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
            });

            modelBuilder.Entity<DiskInventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DiskID).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MaterialType).HasMaxLength(50);
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
                      .HasForeignKey(e => e.DiskId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
