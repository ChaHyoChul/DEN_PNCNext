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

        public async Task<DiskInventory?> GetDiskByBarcodeAsync(string barcode)
        {
            return await _dbContext.DiskInventories
                .FirstOrDefaultAsync(d => d.DiskBarcode == barcode);
        }

        public async Task<IEnumerable<DiskInventory>> GetAllDisksAsync()
        {
            return await _dbContext.DiskInventories.ToListAsync();
        }

        public async Task<DiskInventory> RegisterDiskAsync(DiskInventory disk)
        {
            // 중복 바코드 체크
            var existing = await GetDiskByBarcodeAsync(disk.DiskBarcode);
            if (existing != null)
            {
                throw new InvalidOperationException($"바코드 '{disk.DiskBarcode}'는 이미 등록된 자재입니다.");
            }

            _dbContext.DiskInventories.Add(disk);
            await _dbContext.SaveChangesAsync();
            return disk;
        }

        public async Task UpdateUsedAreaAsync(int diskId, string newAreaJson)
        {
            var disk = await _dbContext.DiskInventories.FindAsync(diskId);
            if (disk == null) throw new KeyNotFoundException("해당 디스크를 찾을 수 없습니다.");

            disk.UsedAreaLayout = newAreaJson;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteDiskAsync(int id)
        {
            var disk = await _dbContext.DiskInventories.FindAsync(id);
            if (disk != null)
            {
                _dbContext.DiskInventories.Remove(disk);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
