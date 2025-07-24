namespace LocationShareApp.Services
{
    public interface IDeviceInfoService
    {
        Task<DeviceInformation> GetDeviceInfoAsync();
        Task<NetworkInformation> GetNetworkInfoAsync();
        Task<SystemInformation> GetSystemInfoAsync();
    }

    public class DeviceInformation
    {
        public string DeviceModel { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class NetworkInformation
    {
        public string NetworkType { get; set; } = string.Empty;
        public string ConnectionType { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public string SignalStrength { get; set; } = string.Empty;
    }

    public class SystemInformation
    {
        public string OperatingSystem { get; set; } = string.Empty;
        public string SystemVersion { get; set; } = string.Empty;
        public long TotalMemory { get; set; }
        public long AvailableMemory { get; set; }
        public long TotalStorage { get; set; }
        public long AvailableStorage { get; set; }
    }
}