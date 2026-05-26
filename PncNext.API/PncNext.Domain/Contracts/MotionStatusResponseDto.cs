using PncNext.Domain.Interfaces;

namespace PncNext.Domain.Contracts
{
    /// <summary>
    /// UI에 전달할 통합 장비 상태 정보 DTO
    /// </summary>
    public class MotionStatusResponseDto
    {
        // 공통 요약 상태 (Ready, Running, Error, NotConnected 등)
        public string OverallStatus { get; set; } = string.Empty;

        // 좌표 정보
        public double[] Position { get; set; } = new double[6];

        // 장비 플래그
        public bool IsServoOn { get; set; }
        public bool IsHomComplete { get; set; }
        public bool IsSpindleRun { get; set; }
        
        // 가공 정보
        public long CurrentLine { get; set; }
        public int ToolNo { get; set; }

        // 에러 정보
        public int GPLErrorCode { get; set; }
        public string LastErrorCode { get; set; } = string.Empty;
        public string LastErrorMessage { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
