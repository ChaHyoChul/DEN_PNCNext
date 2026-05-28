namespace PncNext.Domain.Entities
{
    /// <summary>
    /// 모션 제어기 기본 설정을 관리하는 엔티티 (프로토콜 설정이 채널로 이동됨)
    /// </summary>
    public class MotionControllerConfig
    {
        public int Id { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string ControllerType { get; set; } = "PA"; // PA, INTH, DUMMY
        public bool IsActive { get; set; }

        // 1:N 관계 - 하나의 제어기는 여러 개의 통신 아이템을 가질 수 있음
        public virtual ICollection<CommItemConfig> CommItems { get; set; } = new List<CommItemConfig>();
    }
}
