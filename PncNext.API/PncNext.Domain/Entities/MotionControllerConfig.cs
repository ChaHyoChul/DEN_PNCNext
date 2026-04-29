namespace PncNext.Domain.Entities
{
    public class MotionControllerConfig
    {
        public int Id { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string CommType { get; set; } = string.Empty; // Ethernet or Serial
        public string ProtocolProvider { get; set; } = string.Empty;
        
        // Ethernet settings
        public string? IPAddress { get; set; }
        public int? Port { get; set; }
        
        // Serial settings
        public string? ComPort { get; set; }
        public int? BaudRate { get; set; }
        
        public bool IsActive { get; set; }
    }
}
