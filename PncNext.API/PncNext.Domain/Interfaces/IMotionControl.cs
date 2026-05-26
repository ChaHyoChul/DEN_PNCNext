namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 모션 제어기 및 장비의 실행 상태 정의
    /// </summary>
    public enum MotionStatus
    {
        NotConnected, // 제어기와 통신 연결이 끊긴 상태 (최우선순위)
        NotReady,     // 연결은 되었으나 원점 복귀(Homing)가 되지 않은 상태
        Ready,        // 가공 준비 완료 상태
        Running,      // 가공 또는 동작 중
        Pause,        // 일시 정지 상태
        Error         // 통신은 살아있으나 제어기 또는 하드웨어 에러 발생
    }

    public interface IMotionControl
    {
        /// <summary>
        /// 제어기와의 통신 채널을 엽니다.
        /// </summary>
        Task OpenAsync();

        /// <summary>
        /// 제어기와의 통신 채널을 닫습니다.
        /// </summary>
        Task CloseAsync();

        Task MoveAsync(double x, double y, double z, double a, double b);
        Task StopAsync();
        
        /// <summary>
        /// 현재 장비의 통합 상태를 조회합니다.
        /// </summary>
        Task<MotionStatus> GetStatusAsync();
    }
}
