namespace PncNext.Domain.Entities
{
    public class DiskInventory
    {
        public int Id { get; set; }
        public string DiskBarcode { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public string UsedAreaLayout { get; set; } = "{}"; // JSON Format
    }
}
