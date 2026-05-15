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
        }
    }
}
