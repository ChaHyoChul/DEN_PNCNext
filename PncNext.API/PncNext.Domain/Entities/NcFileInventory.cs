namespace PncNext.Domain.Entities
{
    public class NcFileInventory
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public NcValidationStatus Status { get; set; } = NcValidationStatus.FileLocked;
        
        // 대리키(Surrogate Key) 외래키 연결. 디스크가 지정되지 않거나 삭제 시 null
        public int? DiskSeq { get; set; }
        
        // 파일명에서 추출한 비즈니스 식별자. 자가 치유 시 역추적 용도
        public string? TargetDiskName { get; set; }
        
        public bool IsValidated { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
    }
}
