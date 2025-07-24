namespace LocationShareApp.Services
{
    public interface IBackgroundSyncService
    {
        Task StartSyncAsync();
        Task StopSyncAsync();
        Task SyncLocationDataAsync();
        Task SyncBatteryDataAsync();
        Task SyncDeviceInfoAsync();
        bool IsRunning { get; }
        event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;
    }

    public class SyncStatusEventArgs : EventArgs
    {
        public string Status { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum SyncType
    {
        Location,
        Battery,
        DeviceInfo,
        All
    }
}