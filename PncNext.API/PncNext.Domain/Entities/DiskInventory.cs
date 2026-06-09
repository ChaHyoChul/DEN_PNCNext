namespace PncNext.Domain.Entities
{
    public class DiskInventory
    {
        public int Id { get; set; }
        public string DiskID { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public double Thickness { get; set; }
        public string UsedAreaLayout { get; set; } = "{}"; // JSON Format
        public bool IsDeleted { get; set; }
    }
}
