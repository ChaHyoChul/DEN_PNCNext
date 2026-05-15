namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 통신 포트와 프로토콜이 결합된 채널 인터페이스.
    /// 서비스는 하부 통신 방식이나 구체적인 프로토콜 인코딩 방식을 알 필요 없이 채널에 명령을 내립니다.
    /// </summary>
    public interface IMotionChannel
    {
        bool IsOpen { get; }
        Task OpenAsync();
        Task CloseAsync();
        
        Task MoveAsync(double x, double y, double z, double a, double b);
        Task StopAsync();
        Task<MotionStatus> GetStatusAsync();

        /// <summary>
        /// 특정 서비스(레이저, 오토로더 등)에서 사용하는 커스텀 명령을 전송합니다.
        /// </summary>
        Task<byte[]> SendCustomCommandAsync(string command, params object[] args);
    }
}
