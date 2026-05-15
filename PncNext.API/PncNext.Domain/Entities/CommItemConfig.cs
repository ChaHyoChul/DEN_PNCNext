using System.ComponentModel.DataAnnotations;

namespace PncNext.Domain.Entities
{
    /// <summary>
    /// 모션 제어기의 개별 통신 채널 설정을 관리하는 엔티티 (채널별 프로토콜 할당 추가)
    /// </summary>
    public class CommItemConfig
    {
        public int Id { get; set; }

        public int MotionControllerConfigId { get; set; }
        public virtual MotionControllerConfig? MotionControllerConfig { get; set; }

        [Required]
        [MaxLength(20)]
        public string CommType { get; set; } = "Ethernet"; // Ethernet or Serial

        // Ethernet 설정
        public string? IPAddress { get; set; }
        public int? Port { get; set; }

        // Serial 설정
        public string? ComPort { get; set; }
        public int? BaudRate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Purpose { get; set; } = "CMD"; // CMD, STS, LOG, EVT 등 용도 식별자

        [Required]
        [MaxLength(100)]
        public string ProtocolProvider { get; set; } = "Dummy"; // 채널별 독립적 프로토콜 공급자
    }
}
