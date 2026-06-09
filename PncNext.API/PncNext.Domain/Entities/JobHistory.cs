using System;

namespace PncNext.Domain.Entities
{
    public class JobHistory
    {
        public int Id { get; set; }
        public int NcFileId { get; set; }
        public int DiskId { get; set; }
        public string JobStatus { get; set; } = "Ready"; // Ready, Running, Completed, ErrorStopped, Canceled
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ErrorCode { get; set; }
        public int? ErrorLineNumber { get; set; }
        public int? ParentJobId { get; set; }

        // Navigation properties
        public virtual NcFileInventory? NcFile { get; set; }
        public virtual DiskInventory? Disk { get; set; }
    }
}
