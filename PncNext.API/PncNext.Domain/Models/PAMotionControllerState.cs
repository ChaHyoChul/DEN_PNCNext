namespace PncNext.Domain.Models
{
    /// <summary>
    /// PA 모션 컨트롤러의 상세 상태 데이터를 저장하는 클래스
    /// </summary>
    public class PAMotionControllerState
    {
        // 제어기 상태 (0:idle, 1:running, 2:pause, 3:error)
        public int ControllerState { get; set; }
        
        // 축별 정보 (6축 기준)
        public double[] Position { get; set; } = new double[6];
        public double[] PositionTools { get; set; } = new double[6];
        public double[] Velocity { get; set; } = new double[6];
        
        // I/O 접점 상태 (64포트 기준)
        public bool[] Input { get; set; } = new bool[64];
        public bool[] Output { get; set; } = new bool[64];
        
        // 장비 기본 상태
        public bool IsServoOn { get; set; }
        public bool IsHomComplete { get; set; }
        public long MillingLineNumber { get; set; }
        public int ErrorCode { get; set; } // GPL 에러 코드

        // 툴 및 스핀들 정보
        public int CurrentToolNo { get; set; }
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
    }
}
