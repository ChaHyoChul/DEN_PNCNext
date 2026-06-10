using Microsoft.EntityFrameworkCore;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PncNext.Infrastructure.Services
{
    public class NcFileService : INcFileService
    {
        private readonly AppDbContext _dbContext;
        private readonly string _storagePath;
        private static readonly Regex DiskIdPrefixRegex = new Regex(@"^D\d{4}-", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public NcFileService(AppDbContext dbContext, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _dbContext = dbContext;
            _storagePath = configuration["NcFileStoragePath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NCFiles");
            
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<NcFileInventory> AddNcFileAsync(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("원본 NC 파일을 찾을 수 없습니다.", fullPath);
            }

            var fileName = Path.GetFileName(fullPath);
            var destPath = Path.Combine(_storagePath, fileName);

            // 1. 물리 파일 복사 (덮어쓰기 허용)
            File.Copy(fullPath, destPath, true);

            // 2. DB 등록 확인 (기존 파일이 있으면 업데이트, 없으면 신규)
            var existing = await _dbContext.NcFileInventories
                .FirstOrDefaultAsync(f => f.FileName == fileName && !f.IsDeleted);

            if (existing != null)
            {
                existing.FilePath = destPath;
                existing.IsArchived = false;
                existing.IsValidated = false;
                await _dbContext.SaveChangesAsync();
                return existing;
            }

            var newFile = new NcFileInventory
            {
                FileName = fileName,
                FilePath = destPath,
                IsValidated = false,
                IsArchived = false,
                IsDeleted = false
            };

            _dbContext.NcFileInventories.Add(newFile);
            await _dbContext.SaveChangesAsync();
            return newFile;
        }

        public async Task<IEnumerable<NcFileInventory>> GetAvailableFilesAsync(bool includeArchived = false)
        {
            var query = _dbContext.NcFileInventories
                .Where(f => !f.IsDeleted);

            if (!includeArchived)
            {
                query = query.Where(f => !f.IsArchived);
            }

            return await query.OrderByDescending(f => f.Id).ToListAsync();
        }

        public async Task<NcFileInventory?> GetFileByIdAsync(int id)
        {
            return await _dbContext.NcFileInventories
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        }

        public async Task DeleteFileAsync(int id)
        {
            var file = await _dbContext.NcFileInventories.FindAsync(id);
            if (file != null)
            {
                file.IsDeleted = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task SetArchiveStatusAsync(int id, bool isArchived)
        {
            var file = await _dbContext.NcFileInventories.FindAsync(id);
            if (file != null)
            {
                file.IsArchived = isArchived;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateValidationStatusAsync(int id, bool isValidated)
        {
            var file = await _dbContext.NcFileInventories.FindAsync(id);
            if (file != null)
            {
                file.IsValidated = isValidated;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateNcFileDiskMappingAsync(int ncFileId, int newDiskSeq)
        {
            var ncFile = await _dbContext.NcFileInventories.FindAsync(ncFileId);
            if (ncFile == null) return false;

            // 가드레일 규칙: 가공 중, 완료, 또는 에러 상태의 파일은 변경 불가
            if (ncFile.Status == NcValidationStatus.Processing || 
                ncFile.Status == NcValidationStatus.Completed ||
                ncFile.Status == NcValidationStatus.Error)
            {
                throw new InvalidOperationException("현재 가공 상태인 파일의 디스크 정보는 변경할 수 없습니다.");
            }

            var targetDisk = await _dbContext.DiskInventories.FindAsync(newDiskSeq);
            if (targetDisk == null) throw new ArgumentException("존재하지 않는 디스크 대리키입니다.");

            string targetDiskName = targetDisk.DiskName;

            // 물리적 파일 이름 동기화 (Rename)
            string oldPath = ncFile.FilePath;
            string? directory = Path.GetDirectoryName(oldPath);
            string oldFileName = Path.GetFileName(oldPath);
            
            // 기존 D0000- 접두사 제거 후 새 접두사 추가
            string pureFileName = DiskIdPrefixRegex.Replace(oldFileName, "");
            string newFileName = $"{targetDiskName}-{pureFileName}";
            string newPath = Path.Combine(directory ?? string.Empty, newFileName);

            if (File.Exists(oldPath) && oldPath != newPath)
            {
                // 동일한 이름의 파일이 이미 있는지 확인 (안전 장치)
                if (File.Exists(newPath)) File.Delete(newPath);
                
                File.Move(oldPath, newPath);
                ncFile.FilePath = newPath;
                ncFile.FileName = newFileName;
            }

            ncFile.TargetDiskName = targetDiskName;
            ncFile.Status = NcValidationStatus.Ready;
            ncFile.DiskSeq = targetDisk.Seq;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
