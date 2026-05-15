using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;

namespace PncNext.Infrastructure.Motion.Services
{
    /// <summary>
    /// 장비의 최신 상태 데이터를 메모리에 유지하는 싱글톤 저장소 구현체
    /// </summary>
    public class MotionStateStore : IMotionStateStore
    {
        public PAMotionControllerState PaState { get; } = new PAMotionControllerState();

        public void NotifyStateChanged()
        {
            // 여기서 상태 변경 이벤트를 발생시키거나, 공유 메모리(MMF)에 데이터를 기록하는 로직을 수행할 수 있습니다.
        }
    }
}
