using Microsoft.EntityFrameworkCore;
using SoftLedger.Models;

namespace SoftLedger.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Software> Softwares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Software>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MachineName).HasMaxLength(255);
                entity.Property(e => e.UserName).HasMaxLength(255);
                entity.Property(e => e.SoftwareName).HasMaxLength(500);
                entity.Property(e => e.Version).HasMaxLength(100);
                entity.Property(e => e.Publisher).HasMaxLength(500);
                entity.Property(e => e.InstallDate).HasMaxLength(50);
            });
        }
    }
}
