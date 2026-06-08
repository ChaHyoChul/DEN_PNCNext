using PncNext.Domain.Interfaces;

namespace PncNext.Domain.Models
{
    /// <summary>
    /// PA 모션 컨트롤러의 상세 상태 데이터를 저장하는 클래스
    /// </summary>
    
    public class PAMotionControllerState
    {
        // 제어기 상태 (0:idle, 1:running, 2:pause, 3:error)
        //public int ControllerState { get; set; }
        public MotionStatus ControllerState { get; set; }
        
        // 축별 정보 (6축 기준)
        public double[] Position { get; set; } = new double[6];
        public double[] PositionTools { get; set; } = new double[6];
        public double[] Velocity { get; set; } = new double[6];
        
        // I/O 접점 상태 (64포트 기준)
        public bool[] InputSystem { get; set; } = new bool[16];
        public bool[] OutputSystem { get; set; } = new bool[16];
        public bool[] InputCantops { get; set; } = new bool[32];
        public bool[] OutputCantops { get; set; } = new bool[32];

        // 장비 기본 상태
        public bool IsServoOn { get; set; }
        public bool IsHomComplete { get; set; }
        public long MillingLineNumber { get; set; }
        public int GPLErrorCode { get; set; } // GPL 에러 코드

        // 툴 및 스핀들 정보
        public int CurrentToolNo { get; set; }
        public double CurrentToolLength { get; set; }
        public int SpindleSpeed { get; set; } // spindle rpm
        public int SpindleOverride { get; set; }
        public int SpindleSpeedWithOverride { get; set; }
        public int MotorOverride { get; set; }
        public int MotorFeedrate { get; set; }
        public int MotorFeedrateWithOverride { get; set; }

        // 보드 상태 및 세부 동작 상태
        public int IoBoardState { get; set; } // io board 연결 상태
        public int SpindleBoardState { get; set; } // spindle board 연결 상태
        public bool IsSpindleRun { get; set; }
        public bool IsToolLengthUpdate { get; set; }
        public bool IsDuringToolChange { get; set; }
        public bool IsEmoButtonPressed { get; set; } // EMO 버튼 눌림 상태
        public bool IsMotorMoving { get; set; }
        public bool IsMotorMovingAutoloader { get; set; }
        public bool IsSpindleClampState { get; set; }
        public bool IsPurgeAirState { get; set; }
        public bool IsAirRechargeStarte { get; set; } // 공압 충전 중
        public bool IsM00Command { get; set; } // NC M00 명령에 의한 Pause 여부

        // 제어기 응답 에러 정보
        public string LastErrorCode { get; set; } = string.Empty; // 예: "E9000"
        public string LastErrorMessage { get; set; } = string.Empty; // 예: "ERROR_NETWORK"

        // 신규 추가된 설정 및 결과 데이터
        public double LastMeasureResult { get; set; }
        public string FirmwareVersion { get; set; } = string.Empty;
        public int M28Type { get; set; } // 0:WET, 1:DRY, 2:Z_WET
        public int PurgeAirHoldTime { get; set; }
    }
}
