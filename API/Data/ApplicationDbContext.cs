using Microsoft.EntityFrameworkCore;
using LocationShareApp.API.Models;

namespace LocationShareApp.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<UserBattery> UserBatteries { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 用户表配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.HasIndex(e => e.BindingCode).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastActiveAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 位置表配置
            modelBuilder.Entity<UserLocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Timestamp });
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Locations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 电量表配置
            modelBuilder.Entity<UserBattery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Timestamp });
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.BatteryRecords)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 用户关联表配置
            modelBuilder.Entity<UserConnection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ConnectedUserId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Connections)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.ConnectedUser)
                    .WithMany(u => u.ConnectedBy)
                    .HasForeignKey(e => e.ConnectedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 设备信息表配置
            modelBuilder.Entity<DeviceInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<DeviceInfo>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}