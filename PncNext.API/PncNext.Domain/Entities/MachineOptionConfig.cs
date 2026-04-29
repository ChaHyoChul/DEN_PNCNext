namespace PncNext.Domain.Entities
{
    public class MachineOptionConfig
    {
        public int Id { get; set; }
        
        public int MotionControllerConfigId { get; set; }
        public virtual MotionControllerConfig? MotionControllerConfig { get; set; }

        public string IOMapType { get; set; } = string.Empty;
        public int AxisCount { get; set; }
        public string ToolPocketType { get; set; } = string.Empty;
        
        public bool HasAutoloader { get; set; }
        public bool HasDustCollector { get; set; }
        public bool HasWaterPump { get; set; }
        public int SpindleCount { get; set; }
        public bool HasPurgeAir { get; set; }
        public bool HasAirBlow { get; set; }
    }
}
