namespace PncNext.Domain.Entities
{
    public class NcFileInventory
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsValidated { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
    }
}
