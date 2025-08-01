using Microsoft.AspNetCore.SignalR.Client;

namespace LocationShareApp.Services
{
    public interface ISignalRService
    {
        Task StartConnectionAsync(string token);
        Task StopConnectionAsync();
        Task SendLocationUpdateAsync(double latitude, double longitude, string address, double accuracy);
        Task SendBatteryUpdateAsync(int batteryLevel, bool isCharging);
        event EventHandler<UserLocationUpdatedEventArgs>? UserLocationUpdated;
        event EventHandler<UserBatteryUpdatedEventArgs>? UserBatteryUpdated;
        event EventHandler<UserOnlineEventArgs>? UserOnline;
        event EventHandler<UserOfflineEventArgs>? UserOffline;
        bool IsConnected { get; }
    }

    public class UserLocationUpdatedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class UserBatteryUpdatedEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UserOnlineEventArgs : EventArgs
    {
        public int UserId { get; set; }
    }

    public class UserOfflineEventArgs : EventArgs
    {
        public int UserId { get; set; }
    }
}