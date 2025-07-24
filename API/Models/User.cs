using System.ComponentModel.DataAnnotations;

namespace LocationShareApp.API.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string NickName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Avatar { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string BindingCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = false;
        
        // 导航属性
        public virtual ICollection<UserLocation> Locations { get; set; } = new List<UserLocation>();
        public virtual ICollection<UserBattery> BatteryRecords { get; set; } = new List<UserBattery>();
        public virtual ICollection<UserConnection> Connections { get; set; } = new List<UserConnection>();
        public virtual ICollection<UserConnection> ConnectedBy { get; set; } = new List<UserConnection>();
    }

    public class UserLocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double Accuracy { get; set; }
        
        // 导航属性
        public virtual User User { get; set; } = null!;
    }

    public class UserBattery
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Range(0, 100)]
        public int BatteryLevel { get; set; }
        
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // 导航属性
        public virtual User User { get; set; } = null!;
    }

    public class UserConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ConnectedUserId { get; set; }
        
        [StringLength(50)]
        public string NickName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // 导航属性
        public virtual User User { get; set; } = null!;
        public virtual User ConnectedUser { get; set; } = null!;
    }

    public class DeviceInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [StringLength(100)]
        public string DeviceModel { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string SystemVersion { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string AppVersion { get; set; } = string.Empty;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // 导航属性
        public virtual User User { get; set; } = null!;
    }
}