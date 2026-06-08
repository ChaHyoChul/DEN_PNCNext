namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 모션 제어기 및 장비의 실행 상태 정의
    /// </summary>
    public enum MotionStatus
    {
        NotConnected, // 제어기와 통신 연결이 끊긴 상태
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

        /// <summary>
        /// 입력된 축만 상대 위치로 이동시킵니다.
        /// </summary>
        Task MoveIncrementalAsync(double? x, double? y, double? z, double? a, double? b);

        /// <summary>
        /// 입력된 축만 절대 위치로 이동시킵니다.
        /// </summary>
        Task MoveAbsoluteAsync(double? x, double? y, double? z, double? a, double? b);
        
        /// <summary>
        /// 장비를 정지시킵니다.
        /// </summary>
        /// <param name="mode">정지 모드 (0: 일반 정지, 1: 급정지 등)</param>
        Task StopAsync(int mode);

        /// <summary>
        /// 서보 전원을 제어합니다.
        /// </summary>
        Task SetServoAsync(bool on);

        /// <summary>
        /// 조그 이동을 시작합니다.
        /// </summary>
        Task StartJogAsync(int axis, int direction);

        /// <summary>
        /// 조그 이동을 정지합니다.
        /// </summary>
        Task StopJogAsync();

        /// <summary>
        /// 조그 속도를 설정합니다.
        /// </summary>
        Task SetJogSpeedAsync(int speed);

        /// <summary>
        /// 조그 속도를 읽어옵니다.
        /// </summary>
        Task<int> GetJogSpeedAsync();

        /// <summary>
        /// 디지털 출력을 제어합니다.
        /// </summary>
        Task SetOutputAsync(int bitNo, bool on);

        /// <summary>
        /// 스핀들 시스템을 초기화합니다.
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

        // [2.17, 2.18] 자동 보정 및 설정 변경
        Task StartMeasureAsync(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset);
        Task<double> GetMeasureResultAsync();
        Task SetupSuhoAsync();
        Task SetupSabhoAsync();
        Task SetupSorzAsync();
        Task SetDiskThicknessAsync(double thickness);
        Task SetM28TypeAsync(int type);
        Task<int> GetM28TypeAsync();
        Task ResetHomingStatusAsync();
        Task SetAirParametersAsync(int usingAir, int interval, int usingPurge, int purgeInterval);
        Task SetWaterFlowParametersAsync(int usingWater, int startTimeout, int sensingTimeout);
        Task SetPurgeAirHoldTimeAsync(int holdTime);
        Task<int> GetPurgeAirHoldTimeAsync();

        /// <summary>
        /// 에러 클리어 합니다 
        /// </summary>
        /// <returns></returns>
        Task ErrorResetAsync();

        /// <summary>
        /// 모션 컨트롤러를 초기화 합니다 
        /// </summary>
        Task InitControllerAsync();

        /// <summary>
        /// 장비 원점 복귀를 수행합니다.
        /// </summary>
        Task HomeAsync();
        
        /// <summary>
        /// 장비의 동작 모드를 설정합니다.
        /// </summary>
        /// <param name="mode">"OFF", "AUTO", "STEP", "MDA"</param>
        Task SetModeAsync(string mode);

        Task PauseAsync();

        Task ContinueAsync();

        /// <summary>
        /// G-Code 명령(MDA)을 실행합니다.
        /// </summary>
        Task MdaAsync(string gcode);

        /// <summary>
        /// 현재 장비의 통합 상태를 조회합니다.
        /// </summary>
        Task<MotionStatus> GetStatusAsync();
    }
}
