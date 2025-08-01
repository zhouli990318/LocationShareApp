namespace LocationShareApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string BindingCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class UserLocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Accuracy { get; set; }
    }

    public class UserBattery
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UserConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ConnectedUserId { get; set; }
        public string NickName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}