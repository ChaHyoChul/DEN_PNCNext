using PncNext.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// NC 파일 인벤토리의 조회 및 관리를 담당하는 서비스 인터페이스
    /// </summary>
    public interface INcFileService
    {
        /// <summary>
        /// 활성화된(삭제되지 않은) 모든 NC 파일 목록을 조회합니다.
        /// </summary>
        /// <param name="includeArchived">이미 가공 완료되어 아카이브된 파일 포함 여부</param>
        Task<IEnumerable<NcFileInventory>> GetAvailableFilesAsync(bool includeArchived = false);

        /// <summary>
        /// 특정 ID의 NC 파일 상세 정보를 조회합니다.
        /// </summary>
        Task<NcFileInventory?> GetFileByIdAsync(int id);

        /// <summary>
        /// 외부의 NC 파일을 시스템 저장소로 복사하고 DB에 등록합니다.
        /// </summary>
        /// <param name="fullPath">원본 파일의 전체 경로</param>
        Task<NcFileInventory> AddNcFileAsync(string fullPath);

        /// <summary>
        /// NC 파일의 디스크 매핑 정보를 업데이트하고 물리 파일명을 변경합니다.
        /// </summary>
        Task<bool> UpdateNcFileDiskMappingAsync(int ncFileId, int newDiskId);

        /// <summary>
        /// NC 파일을 논리적으로 삭제 처리합니다. (Soft Delete)
        /// </summary>
        Task DeleteFileAsync(int id);

        /// <summary>
        /// NC 파일의 가공 완료 후 아카이브 상태를 업데이트합니다.
        /// </summary>
        Task SetArchiveStatusAsync(int id, bool isArchived);

        /// <summary>
        /// NC 파일의 G-Code 무결성 검증 결과 상태를 업데이트합니다.
        /// </summary>
        Task UpdateValidationStatusAsync(int id, bool isValidated);
    }
}
