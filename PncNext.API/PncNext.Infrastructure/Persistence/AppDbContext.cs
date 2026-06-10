using Microsoft.EntityFrameworkCore;
using PncNext.Domain.Entities;

namespace PncNext.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MotionControllerConfig> MotionControllerConfigs { get; set; }
        public DbSet<CommItemConfig> CommItemConfigs { get; set; }
        public DbSet<MachineOptionConfig> MachineOptionConfigs { get; set; }

        public DbSet<NcFileInventory> NcFileInventories { get; set; }
        public DbSet<DiskInventory> DiskInventories { get; set; }
        public DbSet<JobHistory> JobHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MotionControllerConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ControllerName).IsRequired().HasMaxLength(100);
                
                // MotionControllerConfig와 CommItemConfig 간의 1:N 관계 설정
                entity.HasMany(e => e.CommItems)
                      .WithOne(c => c.MotionControllerConfig)
                      .HasForeignKey(c => c.MotionControllerConfigId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CommItemConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<MachineOptionConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.MotionControllerConfig)
                      .WithMany()
                      .HasForeignKey(e => e.MotionControllerConfigId);
            });

            modelBuilder.Entity<NcFileInventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
                
                // Status Enum을 DB에 저장할 때 정수가 아닌 문자열로 변환하여 저장
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);
                      
                entity.Property(e => e.TargetDiskName).HasMaxLength(100);
            });

            modelBuilder.Entity<DiskInventory>(entity =>
            {
                entity.HasKey(e => e.Seq);
                entity.Property(e => e.DiskId).IsRequired();
                entity.Property(e => e.DiskName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialType).HasMaxLength(50);
                
                // 부분 고유 인덱스 (Filtered Unique Index): 삭제되지 않은 활성 디스크 간에만 고유성 보장
                entity.HasIndex(e => e.DiskId)
                      .IsUnique()
                      .HasFilter("\"IsDeleted\" = 0");
                      
                entity.HasIndex(e => e.DiskName)
                      .IsUnique()
                      .HasFilter("\"IsDeleted\" = 0");
            });

            modelBuilder.Entity<JobHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobStatus).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.NcFile)
                      .WithMany()
                      .HasForeignKey(e => e.NcFileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Disk)
                      .WithMany()
                      .HasForeignKey(e => e.DiskSeq)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
