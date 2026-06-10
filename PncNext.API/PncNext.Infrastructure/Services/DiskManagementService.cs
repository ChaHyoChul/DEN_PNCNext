using Microsoft.EntityFrameworkCore;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.Infrastructure.Services
{
    public class DiskManagementService : IDiskManagementService
    {
        private readonly AppDbContext _dbContext;
        private readonly ISignalRService _signalRService;

        public DiskManagementService(AppDbContext dbContext, ISignalRService signalRService)
        {
            _dbContext = dbContext;
            _signalRService = signalRService;
        }

        public async Task<DiskInventory?> GetDiskByDiskIdAsync(int diskId)
        {
            return await _dbContext.DiskInventories
                .FirstOrDefaultAsync(d => d.DiskId == diskId && !d.IsDeleted);
        }

        public async Task<IEnumerable<DiskInventory>> GetAllDisksAsync()
        {
            return await _dbContext.DiskInventories
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<DiskInventory> RegisterDiskAsync(DiskInventory disk)
        {
            // Swagger 등 외부 입력의 기본값 오류 보정
            disk.IsDeleted = false;

            // 중복 DiskId 체크 (활성 디스크 내에서만)
            var existing = await GetDiskByDiskIdAsync(disk.DiskId);
            if (existing != null)
            {
                throw new InvalidOperationException($"DiskId '{disk.DiskId}'는 이미 등록된 활성 자재입니다.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 포맷팅 적용
                disk.DiskName = $"D{disk.DiskId:D4}";

                _dbContext.DiskInventories.Add(disk);
                await _dbContext.SaveChangesAsync(); // 새 Seq 발급

                // 자가 치유(Self-healing) 로직
                var orphanedFiles = await _dbContext.NcFileInventories
                    .Where(f => f.Status == NcValidationStatus.InvalidDiskId && f.TargetDiskName == disk.DiskName)
                    .ToListAsync();

                foreach (var file in orphanedFiles)
                {
                    file.DiskSeq = disk.Seq; // 물리 외래키 연결
                    file.Status = NcValidationStatus.Ready;
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. 실시간 멀티 클라이언트 갱신 전파 (SignalR)
                await _signalRService.BroadcastAsync("DiskCreatedAndHealed", new { NewSeq = disk.Seq, AssignedDiskId = disk.DiskId, HealedCount = orphanedFiles.Count });
                return disk;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateUsedAreaAsync(int targetSeq, string newAreaJson)
        {
            var disk = await _dbContext.DiskInventories.FindAsync(targetSeq);
            if (disk == null) throw new KeyNotFoundException("해당 디스크를 찾을 수 없습니다.");

            disk.UsedAreaLayout = newAreaJson;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteDiskAsync(int targetSeq)
        {
            var disk = await _dbContext.DiskInventories.FindAsync(targetSeq);
            if (disk == null) return;

            // 1. 가드레일: 현재 해당 디스크로 가공 중인 파일이 있는지 검증
            bool isCurrentlyMilling = await _dbContext.NcFileInventories
                .AnyAsync(n => n.DiskSeq == targetSeq && n.Status == NcValidationStatus.Processing);

            if (isCurrentlyMilling)
            {
                throw new InvalidOperationException("현재 가공이 진행 중인 디스크는 삭제할 수 없습니다.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 2. 가공 대기(Ready) 상태인 파일들은 외래키를 해제하고 InvalidDiskId로 격리
                var readyFiles = await _dbContext.NcFileInventories
                    .Where(n => n.DiskSeq == targetSeq && n.Status == NcValidationStatus.Ready)
                    .ToListAsync();

                foreach (var file in readyFiles)
                {
                    file.DiskSeq = null; // 물리 연결 단절
                    file.Status = NcValidationStatus.InvalidDiskId;
                }

                // 3. 이력 보존: 소프트 딜리트 처리
                disk.IsDeleted = true;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 4. 실시간 이벤트 전파
                await _signalRService.BroadcastAsync("DiskSoftDeleted", new { RemovedSeq = targetSeq, SeparatedReadyCount = readyFiles.Count });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
