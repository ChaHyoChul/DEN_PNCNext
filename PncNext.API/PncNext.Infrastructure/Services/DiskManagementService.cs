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

        public DiskManagementService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DiskInventory?> GetDiskByDiskIDAsync(string diskId)
        {
            return await _dbContext.DiskInventories
                .FirstOrDefaultAsync(d => d.DiskID == diskId);
        }

        public async Task<IEnumerable<DiskInventory>> GetAllDisksAsync()
        {
            return await _dbContext.DiskInventories
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<DiskInventory> RegisterDiskAsync(DiskInventory disk)
        {
            // 중복 DiskID 체크
            var existing = await GetDiskByDiskIDAsync(disk.DiskID);
            if (existing != null)
            {
                throw new InvalidOperationException($"DiskID '{disk.DiskID}'는 이미 등록된 자재입니다.");
            }

            _dbContext.DiskInventories.Add(disk);
            await _dbContext.SaveChangesAsync();

            // 4단계: 자가 치유(Self-healing) 로직
            // 새로 등록된 디스크 식별자(Id)를 기다리고 있던 InvalidDiskId 상태의 파일들을 검색
            var orphanedFiles = await _dbContext.NcFileInventories
                .Where(f => f.Status == NcValidationStatus.InvalidDiskId && f.TargetDiskId == disk.Id)
                .ToListAsync();

            if (orphanedFiles.Any())
            {
                foreach (var file in orphanedFiles)
                {
                    file.Status = NcValidationStatus.Ready;
                }
                await _dbContext.SaveChangesAsync();
            }

            return disk;
        }

        public async Task UpdateUsedAreaAsync(int diskId, string newAreaJson)
        {
            var disk = await _dbContext.DiskInventories.FindAsync(diskId);
            if (disk == null) throw new KeyNotFoundException("해당 디스크를 찾을 수 없습니다.");

            disk.UsedAreaLayout = newAreaJson;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteDiskAsync(int diskId)
        {
            // 1. 가드레일: 현재 해당 디스크로 가공 중인 파일이 있는지 검증
            bool isCurrentlyMilling = await _dbContext.NcFileInventories
                .AnyAsync(n => n.TargetDiskId == diskId && n.Status == NcValidationStatus.Processing);

            if (isCurrentlyMilling)
            {
                throw new InvalidOperationException("현재 가공이 진행 중인 디스크는 삭제할 수 없습니다.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 2. 가공 대기(Ready) 상태인 파일들은 InvalidDiskId로 격리하여 상태 역전이
                var readyFiles = await _dbContext.NcFileInventories
                    .Where(n => n.TargetDiskId == diskId && n.Status == NcValidationStatus.Ready)
                    .ToListAsync();

                foreach (var file in readyFiles)
                {
                    file.Status = NcValidationStatus.InvalidDiskId;
                }

                // 3. 이력 보존: 소프트 딜리트 처리
                var disk = await _dbContext.DiskInventories.FindAsync(diskId);
                if (disk != null)
                {
                    disk.IsDeleted = true;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
