namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 통신 포트와 프로토콜이 결합된 채널 인터페이스.
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
        /// 특정 서비스에서 사용하는 커스텀 명령을 전송합니다.
        /// </summary>
        Task<byte[]> SendCustomCommandAsync(string command, params object[] args);

        /// <summary>
        /// 해당 채널에 설정된 프로토콜 객체를 가져옵니다.
        /// </summary>
        IMotionProtocol Protocol { get; }

        /// <summary>
        /// 하부 통신 포트의 원시 읽기 기능을 제공합니다.
        /// </summary>
        Task<byte[]> ReceiveRawAsync();
    }
}
