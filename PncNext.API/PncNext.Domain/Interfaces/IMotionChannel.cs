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
        /// 서보를 활성화(ON)하거나 비활성화(OFF)합니다.
        /// </summary>
        Task SetServoAsync(bool on);

        /// <summary>
        /// 특정 축의 조그 이동을 시작합니다.
        /// </summary>
        /// <param name="axis">축 번호 (0:X, 1:Y, 2:Z...)</param>
        /// <param name="direction">방향 (1:+, 0:-)</param>
        Task StartJogAsync(int axis, int direction);

        /// <summary>
        /// 조그 이동을 즉시 중지합니다.
        /// </summary>
        Task StopJogAsync();

        /// <summary>
        /// 조그 속도를 설정합니다. (0~100)
        /// </summary>
        Task SetJogSpeedAsync(int speed);

        /// <summary>
        /// 설정된 조그 속도를 읽어옵니다.
        /// </summary>
        Task<int> GetJogSpeedAsync();

        /// <summary>
        /// 특정 출력 비트의 상태를 제어합니다.
        /// </summary>
        /// <param name="bitNo">비트 번호</param>
        /// <param name="on">On/Off 여부</param>
        Task SetOutputAsync(int bitNo, bool on);

        /// <summary>
        /// 스핀들 통신 보드를 초기화합니다.
        /// </summary>
        Task InitSpindleAsync();

        // [2단계] 파라미터 및 데이터 동기화
        Task<double[]> GetCoordinateOffsetAsync(int index);
        Task SetCoordinateOffsetAsync(int index, double[] values);
        Task<double[]> GetTeachingPointAsync(int index);
        Task SetTeachingPointAsync(int index, double[] values);
        Task<double> GetZOriginOffsetAsync();
        Task SetZOriginOffsetAsync(double offset);
        Task<int> GetToolSensingHighSpeedAsync();
        Task SetToolSensingHighSpeedAsync(int speed);
        Task<int> GetToolSensingLowSpeedAsync();
        Task SetToolSensingLowSpeedAsync(int speed);
        Task<double> GetToolSensingMarginAsync();
        Task SetToolSensingMarginAsync(double margin);
        Task<double> GetToolPocketPutOffsetAsync();
        Task SetToolPocketPutOffsetAsync(double offset);
        Task<double[]> GetSoftLimitPositiveAsync();
        Task SetSoftLimitPositiveAsync(double[] values);
        Task<double[]> GetSoftLimitNegativeAsync();
        Task SetSoftLimitNegativeAsync(double[] values);

        // [3단계] 시스템 설정 및 정보 조회
        Task<string> GetControllerIpAsync();
        Task SetControllerIpAsync(string ip);
        Task<string> GetIoBoardIpAsync();
        Task SetIoBoardIpAsync(string ip);
        Task<string> GetFirmwareVersionAsync();
        Task SaveToFlashAsync();
        Task RestoreToolInfoAsync(int toolNo, double length, bool updated);

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
        /// 하부 통신 포트의 원시 읽기 기능을 제공합니다.
        /// </summary>
        Task<byte[]> ReceiveRawAsync();
    }
}
