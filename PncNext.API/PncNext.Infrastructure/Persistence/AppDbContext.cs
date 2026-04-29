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
        public DbSet<MachineOptionConfig> MachineOptionConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MotionControllerConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ControllerName).IsRequired().HasMaxLength(100);
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
