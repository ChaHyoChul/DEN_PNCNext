namespace PncNext.Domain.Models
{
    /// <summary>
    /// 명령어 전송을 위한 정보를 담는 데이터 전송 객체 (DTO)
    /// </summary>
    public class MotionCommandInfo
    {
        /// <summary>
        /// 명령어 식별 키 (예: "RND_CDT", "MOV")
        /// </summary>
        public string CommandKey { get; set; } = string.Empty;

        /// <summary>
        /// 하드웨어로 전송할 실제 바이트 배열
        /// </summary>
        public byte[] Payload { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 해당 명령의 응답 대기 제한 시간 (밀리초)
        /// </summary>
        public int TimeoutMs { get; set; } = 3000; // 기본 3초
    }
}
