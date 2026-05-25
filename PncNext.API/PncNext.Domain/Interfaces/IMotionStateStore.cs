using PncNext.Domain.Models;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 모든 제어기의 최신 상태 데이터를 메모리에 유지하는 싱글톤 저장소 인터페이스
    /// </summary>
    public interface IMotionStateStore
    {
        /// <summary>
        /// PA 제어기의 상세 상태
        /// </summary>
        PAMotionControllerState PaState { get; }
        
        // 향후 INTH 상태 등 추가 가능 (예: INTHMotionControllerState InthState { get; } )
        
        /// <summary>
        /// 상태 변경을 알리고 공유 메모리 등으로 데이터를 내보냅니다.
        /// </summary>
        /// <param name="controllerType">상태가 변경된 제어기 식별자 ("PA", "INTH" 등)</param>
        void NotifyStateChanged(string controllerType);
    }
}
