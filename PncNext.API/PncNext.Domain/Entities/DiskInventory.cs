using System.ComponentModel;

namespace PncNext.Domain.Entities
{
    public class DiskInventory
    {
        public int Seq { get; set; }
        public int DiskId { get; set; }
        public string DiskName { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public double Thickness { get; set; }
        public string UsedAreaLayout { get; set; } = "{}"; // JSON Format

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
