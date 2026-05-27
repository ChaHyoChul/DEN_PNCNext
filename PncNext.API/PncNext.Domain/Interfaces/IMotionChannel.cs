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

        /// <summary>
        /// 장비 정지 명령을 전송합니다.
        /// </summary>
        Task StopAsync(int mode);

        /// <summary>
        /// 에러 리셋 명령을 전송합니다 
        /// </summary>
        Task ErrorResetAsync();

        /// <summary>
        /// 모션 컨트롤러를 초기화 합니다 
        /// </summary>
        Task InitControllerAsync();

        /// <summary>
        /// 장비 원점 복귀 명령을 전송합니다.
        /// </summary>
        Task HomeAsync();

        /// <summary>
        /// 장비의 동작 모드 전환 명령을 전송합니다.
        /// </summary>
        Task SetModeAsync(string mode);

        Task PauseAsync();

        Task ContinueAsync();

        /// <summary>
        /// G-Code 명령(MDA) 전송을 수행합니다.
        /// </summary>
        Task MdaAsync(string gcode);

        /// <summary>
        /// 입력된 축만 상대 위치로 이동시키는 명령을 전송합니다.
        /// </summary>
        Task MoveIncrementalAsync(double? x, double? y, double? z, double? a, double? b);

        /// <summary>
        /// 입력된 축만 절대 위치로 이동시키는 명령을 전송합니다.
        /// </summary>
        Task MoveAbsoluteAsync(double? x, double? y, double? z, double? a, double? b);

        Task<MotionStatus> GetStatusAsync();

        /// <summary>
        /// 하부 통신 포트를 통해 장비의 전체 상태 데이터를 요청하고 원시 바이트로 읽어옵니다.
        /// </summary>
        Task<byte[]> ReadFullStatusAsync();

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
