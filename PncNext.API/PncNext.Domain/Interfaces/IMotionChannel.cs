namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 통신 포트와 프로토콜이 결합된 채널 인터페이스.
    /// </summary>
    public interface IMotionChannel
    {
        bool IsOpen { get; }
        
        /// <summary>
        /// 통신 장애 또는 타임아웃 발생 시 true가 됩니다.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// 데이터 수신 시 상위 레이어에 알리는 이벤트.
        /// </summary>
        event EventHandler<byte[]> MessageReceived;

        Task OpenAsync();
        Task CloseAsync();
        
        /// <summary>
        /// 에러 상태(Fault)를 해제하고 재연결을 준비합니다.
        /// </summary>
        void ResetFault();

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
        /// 하부 통신 포트를 통해 장비의 전체 상태 데이터를 요청하고 원시 바이트로 읽어옵니다.
        /// </summary>
        Task<byte[]> ReadFullStatusAsync();
    }
}
