using Microsoft.Maui.Devices.Sensors;

namespace LocationShareApp.Services
{
    public class BackgroundSyncService : IBackgroundSyncService
    {
        private readonly IApiService _apiService;
        private readonly ILocationService _locationService;
        private readonly IBatteryService _batteryService;
        private readonly IDeviceInfoService _deviceInfoService;
        private readonly ISignalRService _signalRService;
        
        private Timer? _syncTimer;
        private bool _isRunning;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5); // 每5分钟同步一次

        public event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;

        public bool IsRunning => _isRunning;

        public BackgroundSyncService(
            IApiService apiService,
            ILocationService locationService,
            IBatteryService batteryService,
            IDeviceInfoService deviceInfoService,
            ISignalRService signalRService)
        {
            _apiService = apiService;
            _locationService = locationService;
            _batteryService = batteryService;
            _deviceInfoService = deviceInfoService;
            _signalRService = signalRService;
        }

        public async Task StartSyncAsync()
        {
            if (_isRunning) return;

            try
            {
                _isRunning = true;
                
                // 立即执行一次同步
                await PerformSyncAsync();
                
                // 启动定时器
                _syncTimer = new Timer(async _ => await PerformSyncAsync(), null, _syncInterval, _syncInterval);
                
                OnSyncStatusChanged("后台同步服务已启动", true);
            }
            catch (Exception ex)
            {
                _isRunning = false;
                OnSyncStatusChanged($"启动后台同步服务失败: {ex.Message}", false, ex.Message);
            }
        }

        public async Task StopSyncAsync()
        {
            if (!_isRunning) return;

            try
            {
                _isRunning = false;
                _syncTimer?.Dispose();
                _syncTimer = null;
                
                OnSyncStatusChanged("后台同步服务已停止", true);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"停止后台同步服务失败: {ex.Message}", false, ex.Message);
            }
        }

        public async Task SyncLocationDataAsync()
        {
            try
            {
                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    var address = await GetAddressFromLocationAsync(location);
                    
                    // 同步到API
                    var apiResult = await _apiService.UpdateLocationAsync(
                        location.Latitude, 
                        location.Longitude, 
                        address, 
                        location.Accuracy ?? 0);

                    if (apiResult.IsSuccess)
                    {
                        // 通过SignalR实时推送
                        await _signalRService.SendLocationUpdateAsync(
                            location.Latitude, 
                            location.Longitude, 
                            address, 
                            location.Accuracy ?? 0);
                        
                        OnSyncStatusChanged("位置数据同步成功", true);
                    }
                    else
                    {
                        OnSyncStatusChanged("位置数据同步失败", false, apiResult.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"位置数据同步异常: {ex.Message}", false, ex.Message);
            }
        }

        public async Task SyncBatteryDataAsync()
        {
            try
            {
                var batteryLevel = await _batteryService.GetBatteryLevelAsync();
                var isCharging = await _batteryService.IsBatteryChargingAsync();
                
                // 同步到API
                var apiResult = await _apiService.UpdateBatteryAsync((int)(batteryLevel * 100), isCharging);
                
                if (apiResult.IsSuccess)
                {
                    // 通过SignalR实时推送
                    await _signalRService.SendBatteryUpdateAsync((int)(batteryLevel * 100), isCharging);
                    
                    OnSyncStatusChanged("电量数据同步成功", true);
                }
                else
                {
                    OnSyncStatusChanged("电量数据同步失败", false, apiResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"电量数据同步异常: {ex.Message}", false, ex.Message);
            }
        }

        public async Task SyncDeviceInfoAsync()
        {
            try
            {
                var deviceInfo = await _deviceInfoService.GetDeviceInfoAsync();
                var networkInfo = await _deviceInfoService.GetNetworkInfoAsync();
                
                // 这里可以添加设备信息同步到服务器的逻辑
                // 目前设备信息主要用于本地显示
                
                OnSyncStatusChanged("设备信息同步成功", true);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"设备信息同步异常: {ex.Message}", false, ex.Message);
            }
        }

        private async Task PerformSyncAsync()
        {
            if (!_isRunning) return;

            try
            {
                // 检查SignalR连接状态
                if (!_signalRService.IsConnected)
                {
                    OnSyncStatusChanged("SignalR连接断开，跳过本次同步", false, "连接断开");
                    return;
                }

                // 同步位置数据
                await SyncLocationDataAsync();
                
                // 等待一小段时间避免请求过于频繁
                await Task.Delay(1000);
                
                // 同步电量数据
                await SyncBatteryDataAsync();
                
                // 等待一小段时间
                await Task.Delay(1000);
                
                // 同步设备信息（频率较低）
                if (DateTime.Now.Minute % 15 == 0) // 每15分钟同步一次设备信息
                {
                    await SyncDeviceInfoAsync();
                }
                
                OnSyncStatusChanged("数据同步完成", true);
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"数据同步失败: {ex.Message}", false, ex.Message);
            }
        }

        private async Task<string> GetAddressFromLocationAsync(Location location)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                
                if (placemark != null)
                {
                    return $"{placemark.Locality} {placemark.Thoroughfare}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取地址失败: {ex.Message}");
            }

            return $"{location.Latitude:F6}, {location.Longitude:F6}";
        }

        private void OnSyncStatusChanged(string status, bool isSuccess, string? errorMessage = null)
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
            {
                Status = status,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.Now
            });
            
            System.Diagnostics.Debug.WriteLine($"[BackgroundSync] {status} - {(isSuccess ? "成功" : "失败")} {errorMessage}");
        }
    }
}