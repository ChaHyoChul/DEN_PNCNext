using PncNext.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 디스크 자재(Puck/Block)의 라이프사이클 및 재고 관리를 담당하는 서비스 인터페이스
    /// </summary>
    public interface IDiskManagementService
    {
        /// <summary>
        /// 바코드를 통해 특정 디스크 정보를 조회합니다.
        /// </summary>
        Task<DiskInventory?> GetDiskByBarcodeAsync(string barcode);

        /// <summary>
        /// 현재 등록된 모든 디스크 목록을 조회합니다.
        /// </summary>
        Task<IEnumerable<DiskInventory>> GetAllDisksAsync();

        /// <summary>
        /// 새로운 디스크 자재를 시스템에 등록합니다.
        /// </summary>
        Task<DiskInventory> RegisterDiskAsync(DiskInventory disk);

        /// <summary>
        /// 가공 완료 후 디스크의 사용된 영역 레이아웃(JSON)을 업데이트합니다.
        /// </summary>
        Task UpdateUsedAreaAsync(int diskId, string newAreaJson);

        /// <summary>
        /// 디스크 정보를 삭제합니다.
        /// </summary>
        Task DeleteDiskAsync(int id);
    }
}
