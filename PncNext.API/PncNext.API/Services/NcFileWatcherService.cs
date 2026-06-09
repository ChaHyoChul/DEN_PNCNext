using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PncNext.Domain.Entities;
using PncNext.Infrastructure.Persistence;

namespace PncNext.API.Services
{
    public class NcFileWatcherService : BackgroundService
    {
        private readonly ILogger<NcFileWatcherService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _watchFolderPath;
        private FileSystemWatcher? _watcher;
        
        // 정규식: 파일명 시작이 D로 시작하고 숫자가 온 뒤 하이픈(-)이 오는 패턴 (예: D0005-...)
        private static readonly Regex DiskIdRegex = new Regex(@"^D(\d+)-", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public NcFileWatcherService(
            ILogger<NcFileWatcherService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            // 기본 감시 폴더 설정 (appsettings.json에서 "NcFileStoragePath" 키로 재정의 가능)
            _watchFolderPath = configuration["NcFileStoragePath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NCFiles");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!Directory.Exists(_watchFolderPath))
                {
                    Directory.CreateDirectory(_watchFolderPath);
                    _logger.LogInformation($"감시 폴더를 생성했습니다: {_watchFolderPath}");
                }

                _watcher = new FileSystemWatcher(_watchFolderPath)
                {
                    Filter = "*.nc",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                _watcher.Created += OnFileCreated;
                _logger.LogInformation($"NC 파일 폴더 감시를 시작합니다: {_watchFolderPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NcFileWatcherService 초기화 중 오류가 발생했습니다.");
            }

            return Task.CompletedTask;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"새로운 NC 파일 감지됨: {e.FullPath}");

            // 파일 복사 완료 대기 및 DB 등록 처리를 비동기로 실행하여 워처 이벤트를 차단하지 않음
            _ = ProcessNewFileAsync(e.FullPath);
        }

        private async Task ProcessNewFileAsync(string filePath)
        {
            try
            {
                // 1. 파일 복사가 완전히 끝날 때까지 락 검사 및 대기
                bool isUnlocked = await WaitForFileUnlockAsync(filePath, TimeSpan.FromSeconds(30));
                
                if (!isUnlocked)
                {
                    _logger.LogWarning($"파일 락 대기 시간 초과: {filePath}. 파일이 손상되었거나 다른 프로세스가 너무 오래 점유하고 있습니다.");
                    return;
                }

                _logger.LogInformation($"파일 복사 완료 및 락 해제 확인: {filePath}");

                // 2. DB 등록 및 스마트 덮어쓰기 로직 (Scoped 서비스 사용)
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var fileName = Path.GetFileName(filePath);

                    // 이름이 동일하고 아직 삭제되지 않은 기존 파일 검색
                    var existingFile = dbContext.NcFileInventories
                        .FirstOrDefault(f => f.FileName == fileName && !f.IsDeleted);

                    int? parentJobId = null;

                    if (existingFile != null)
                    {
                        // 기존 파일과 연관된 가장 최근의 JobHistory 조회
                        var latestJob = dbContext.JobHistories
                            .Where(j => j.NcFileId == existingFile.Id)
                            .OrderByDescending(j => j.StartTime)
                            .FirstOrDefault();

                        if (latestJob != null)
                        {
                            if (latestJob.JobStatus == "Running" || latestJob.JobStatus == "Queued")
                            {
                                // 제어 보호: 현재 가공 중이거나 대기열에 있으면 덮어쓰기 거부
                                _logger.LogWarning($"[제어 보호] '{fileName}' 파일은 현재 가공 중이거나 대기 상태입니다. 덮어쓰기가 취소되었습니다.");
                                // 물리적으로 새로 인입된 파일을 삭제 (또는 격리)하여 시스템 혼란 방지
                                File.Delete(filePath);
                                return;
                            }
                            
                            if (latestJob.JobStatus == "ErrorStopped" || latestJob.JobStatus == "Canceled")
                            {
                                // 계보 추적을 위해 ParentJobId 확보
                                parentJobId = latestJob.Id;
                            }
                        }

                        // 이력 보존형 아카이빙 처리
                        ArchiveExistingFile(existingFile, dbContext);
                    }

                    // 신규 파일 엔티티 생성을 위한 분석
                    var newStatus = NcValidationStatus.Ready;
                    int? targetDiskId = null;

                    // 1. 파일명에서 DiskID 추출 (D0000- 패턴)
                    var match = DiskIdRegex.Match(fileName);
                    if (match.Success)
                    {
                        if (int.TryParse(match.Groups[1].Value, out int diskId))
                        {
                            targetDiskId = diskId;
                            // 2. DB에서 실제 디스크 존재 여부 확인
                            var diskExists = await dbContext.DiskInventories.AnyAsync(d => d.Id == diskId);
                            if (!diskExists)
                            {
                                newStatus = NcValidationStatus.InvalidDiskId;
                                _logger.LogWarning($"[분석] 파일명에 디스크 ID({diskId})가 있으나 DB에 등록되지 않았습니다: {fileName}");
                            }
                        }
                    }
                    else
                    {
                        newStatus = NcValidationStatus.MissingDiskInfo;
                        _logger.LogWarning($"[분석] 파일명에 디스크 식별 접두사가 없습니다: {fileName}");
                    }

                    // 3. 신규 파일 엔티티 등록
                    var newFileEntry = new NcFileInventory
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        Status = newStatus,
                        TargetDiskId = targetDiskId,
                        IsValidated = false,
                        IsArchived = false,
                        IsDeleted = false
                    };

                    dbContext.NcFileInventories.Add(newFileEntry);
                    await dbContext.SaveChangesAsync(); // 새 파일 ID 발급
                    
                    _logger.LogInformation($"DB 등록 완료: {fileName} (ID: {newFileEntry.Id}, Status: {newFileEntry.Status})");

                    // 덮어쓰기된 재가공 파일이라면, 초기 JobHistory를 연결용으로 임시 생성 (선택 사항)
                    // 보통은 가공 시작 시점에 JobHistory를 생성하지만, 이력 연결을 명확히 하기 위해
                    // Ready 상태로 미리 생성해 둘 수 있습니다.
                    if (parentJobId.HasValue)
                    {
                        var linkJob = new JobHistory
                        {
                            NcFileId = newFileEntry.Id,
                            DiskId = existingFile != null ? dbContext.JobHistories.FirstOrDefault(j => j.Id == parentJobId)?.DiskId ?? 0 : 0,
                            JobStatus = "Ready",
                            StartTime = DateTime.UtcNow,
                            ParentJobId = parentJobId
                        };
                        
                        // DiskId가 0(유효하지 않음)인 경우는 제외하고 등록
                        if (linkJob.DiskId > 0)
                        {
                            dbContext.JobHistories.Add(linkJob);
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"이전 에러 기록(JobId:{parentJobId})과 연결된 새 작업(JobId:{linkJob.Id})이 등록되었습니다.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"파일 처리 중 예외 발생: {filePath}");
            }
        }

        private void ArchiveExistingFile(NcFileInventory existingFile, AppDbContext dbContext)
        {
            try
            {
                // 물리 파일 아카이브 처리
                var directory = Path.GetDirectoryName(existingFile.FilePath);
                var archiveDir = Path.Combine(directory ?? string.Empty, "_Archive");
                
                if (!Directory.Exists(archiveDir))
                {
                    Directory.CreateDirectory(archiveDir);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var newFileName = $"{timestamp}_{existingFile.FileName}";
                var archivePath = Path.Combine(archiveDir, newFileName);

                if (File.Exists(existingFile.FilePath))
                {
                    // 예전 파일을 아카이브 폴더로 이동 (이름 변경)
                    File.Move(existingFile.FilePath, archivePath);
                    _logger.LogInformation($"기존 파일 아카이빙 완료: {archivePath}");
                }

                // 기존 DB 레코드를 논리적 삭제 처리
                existingFile.IsDeleted = true;
                existingFile.IsArchived = true;
                existingFile.FilePath = archivePath; // 아카이브된 새 경로로 업데이트
                
                // dbContext.SaveChangesAsync()는 호출자(ProcessNewFileAsync)에서 일괄 처리
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"기존 파일 아카이빙 중 오류 발생: {existingFile.FileName}");
            }
        }


        /// <summary>
        /// 파일이 다른 프로세스(복사 중인 프로세스 등)에 의해 사용 중인지 확인하고 대기합니다.
        /// </summary>
        private async Task<bool> WaitForFileUnlockAsync(string filePath, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;
            var pollInterval = TimeSpan.FromMilliseconds(500);

            while (DateTime.UtcNow - startTime < timeout)
            {
                try
                {
                    // 독점적 쓰기 권한으로 파일을 열어봅니다.
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        if (stream.Length > 0)
                        {
                            // 파일 크기가 0보다 크고 독점 접근이 가능하면 복사가 끝난 것으로 간주합니다.
                            return true;
                        }
                    }
                }
                catch (IOException)
                {
                    // IOException이 발생하면 파일이 사용 중임을 의미합니다. (파일 잠금)
                    // 무시하고 다음 루프에서 재시도합니다.
                }
                catch (UnauthorizedAccessException)
                {
                    // 권한 없음 에러 처리
                }

                await Task.Delay(pollInterval);
            }

            return false; // 타임아웃
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
