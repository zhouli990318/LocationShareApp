namespace LocationShareApp.Services
{
    public class DeviceInfoService : IDeviceInfoService
    {
        public async Task<DeviceInformation> GetDeviceInfoAsync()
        {
            try
            {
                return await Task.FromResult(new DeviceInformation
                {
                    DeviceModel = DeviceInfo.Current.Model,
                    DeviceName = DeviceInfo.Current.Name,
                    Manufacturer = DeviceInfo.Current.Manufacturer,
                    Platform = DeviceInfo.Current.Platform.ToString(),
                    Version = DeviceInfo.Current.VersionString,
                    AppVersion = AppInfo.Current.VersionString
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取设备信息失败: {ex.Message}");
                return new DeviceInformation();
            }
        }

        public async Task<NetworkInformation> GetNetworkInfoAsync()
        {
            try
            {
                var connectivity = Connectivity.Current;
                var networkAccess = connectivity.NetworkAccess;
                var profiles = connectivity.ConnectionProfiles;

                var networkType = "未知";
                var connectionType = "未连接";

                if (profiles.Contains(ConnectionProfile.WiFi))
                {
                    networkType = "WiFi";
                    connectionType = "无线网络";
                }
                else if (profiles.Contains(ConnectionProfile.Cellular))
                {
                    networkType = "移动网络";
                    connectionType = "蜂窝网络";
                }
                else if (profiles.Contains(ConnectionProfile.Ethernet))
                {
                    networkType = "以太网";
                    connectionType = "有线网络";
                }

                return await Task.FromResult(new NetworkInformation
                {
                    NetworkType = networkType,
                    ConnectionType = connectionType,
                    IsConnected = networkAccess == NetworkAccess.Internet,
                    SignalStrength = "良好" // 简化处理，实际应用中可以获取具体信号强度
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取网络信息失败: {ex.Message}");
                return new NetworkInformation();
            }
        }

        public async Task<SystemInformation> GetSystemInfoAsync()
        {
            try
            {
                // 注意：MAUI中获取内存和存储信息有限，这里提供基本实现
                return await Task.FromResult(new SystemInformation
                {
                    OperatingSystem = DeviceInfo.Current.Platform.ToString(),
                    SystemVersion = DeviceInfo.Current.VersionString,
                    TotalMemory = 0, // 需要平台特定实现
                    AvailableMemory = 0, // 需要平台特定实现
                    TotalStorage = 0, // 需要平台特定实现
                    AvailableStorage = 0 // 需要平台特定实现
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取系统信息失败: {ex.Message}");
                return new SystemInformation();
            }
        }
    }
}