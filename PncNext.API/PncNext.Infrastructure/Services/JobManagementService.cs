using Microsoft.EntityFrameworkCore;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PncNext.Infrastructure.Services
{
    public class JobManagementService : IJobManagementService
    {
        private readonly AppDbContext _dbContext;
        private readonly ISignalRService _signalRService;

        public JobManagementService(AppDbContext dbContext, ISignalRService signalRService)
        {
            _dbContext = dbContext;
            _signalRService = signalRService;
        }

        public async Task<JobHistory> StartJobAsync(int ncFileId, int diskSeq)
        {
            // 1. 가드레일 검사
            var ncFile = await _dbContext.NcFileInventories.FindAsync(ncFileId);
            if (ncFile == null) throw new KeyNotFoundException("대상 NC 파일을 찾을 수 없습니다.");
            if (ncFile.Status != NcValidationStatus.Ready) 
                throw new InvalidOperationException($"파일이 가공 준비 상태가 아닙니다. (현재 상태: {ncFile.Status})");

            var disk = await _dbContext.DiskInventories.FindAsync(diskSeq);
            if (disk == null || disk.IsDeleted) throw new KeyNotFoundException("대상 디스크가 존재하지 않거나 이미 삭제되었습니다.");

            // 현재 가공 중인 다른 작업이 있는지 체크
            bool isAnyProcessing = await _dbContext.NcFileInventories.AnyAsync(f => f.Status == NcValidationStatus.Processing);
            if (isAnyProcessing) throw new InvalidOperationException("현재 다른 가공 작업이 진행 중입니다.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 2. JobHistory 레코드 생성
                var job = new JobHistory
                {
                    NcFileId = ncFileId,
                    DiskSeq = diskSeq,
                    JobStatus = "Running",
                    StartTime = DateTime.UtcNow
                };
                _dbContext.JobHistories.Add(job);

                // 3. NC 파일 상태 변경
                ncFile.Status = NcValidationStatus.Processing;
                ncFile.DiskSeq = diskSeq;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 4. 실시간 동기화
                await _signalRService.BroadcastAsync("JobStarted", new { JobId = job.Id, FileName = ncFile.FileName, DiskName = disk.DiskName });

                return job;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CompleteJobAsync(int jobId, string newUsedAreaJson)
        {
            var job = await _dbContext.JobHistories
                .Include(j => j.NcFile)
                .Include(j => j.Disk)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) throw new KeyNotFoundException("해당 작업 이력을 찾을 수 없습니다.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. 작업 상태 갱신
                job.JobStatus = "Completed";
                job.EndTime = DateTime.UtcNow;

                // 2. NC 파일 상태 갱신
                if (job.NcFile != null)
                {
                    job.NcFile.Status = NcValidationStatus.Completed;
                }

                // 3. 디스크 사용 영역 갱신
                if (job.Disk != null)
                {
                    job.Disk.UsedAreaLayout = newUsedAreaJson;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 4. 실시간 동기화
                await _signalRService.BroadcastAsync("JobCompleted", new { JobId = jobId, DiskSeq = job.DiskSeq });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task FailJobAsync(int jobId, string errorCode, int errorLineNumber)
        {
            var job = await _dbContext.JobHistories
                .Include(j => j.NcFile)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) throw new KeyNotFoundException("해당 작업 이력을 찾을 수 없습니다.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                job.JobStatus = "ErrorStopped";
                job.EndTime = DateTime.UtcNow;
                job.ErrorCode = errorCode;
                job.ErrorLineNumber = errorLineNumber;

                if (job.NcFile != null)
                {
                    job.NcFile.Status = NcValidationStatus.Error;
                    job.NcFile.LastErrorLine = errorLineNumber; // 파일 레벨에 중단 라인 동기화
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _signalRService.BroadcastAsync("JobFailed", new { JobId = jobId, ErrorCode = errorCode, Line = errorLineNumber });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelJobAsync(int jobId)
        {
            var job = await _dbContext.JobHistories
                .Include(j => j.NcFile)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) throw new KeyNotFoundException("해당 작업 이력을 찾을 수 없습니다.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                job.JobStatus = "Canceled";
                job.EndTime = DateTime.UtcNow;

                if (job.NcFile != null)
                {
                    // 취소된 파일도 다시 수정 후 투입해야 하므로 Error와 유사하게 처리 (또는 Ready로 되돌릴지 정책 결정 필요)
                    // 사양상 재분석이 필요하므로 Error 상태로 두어 조치를 유도
                    job.NcFile.Status = NcValidationStatus.Error;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _signalRService.BroadcastAsync("JobCanceled", new { JobId = jobId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<JobHistory>> GetRecentJobsAsync(int count = 50)
        {
            return await _dbContext.JobHistories
                .Include(j => j.NcFile)
                .Include(j => j.Disk)
                .OrderByDescending(j => j.StartTime)
                .Take(count)
                .ToListAsync();
        }
    }
}
