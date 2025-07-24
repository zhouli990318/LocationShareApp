using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Authentication;

namespace LocationShareApp.Services
{
    public class LocationService : ILocationService
    {
        private bool _isTracking;
        private CancellationTokenSource? _cancelTokenSource;

        public event EventHandler<LocationEventArgs>? LocationChanged;

        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var hasPermission = await RequestLocationPermissionAsync();
                if (!hasPermission)
                    return null;

                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                _cancelTokenSource = new CancellationTokenSource();
                var location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取位置失败: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"请求位置权限失败: {ex.Message}");
                return false;
            }
        }

        public async Task StartLocationUpdatesAsync()
        {
            if (_isTracking)
                return;

            var hasPermission = await RequestLocationPermissionAsync();
            if (!hasPermission)
                return;

            _isTracking = true;
            _cancelTokenSource = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (_isTracking && !_cancelTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var location = await GetCurrentLocationAsync();
                        if (location != null)
                        {
                            var address = await GetAddressFromLocationAsync(location);
                            LocationChanged?.Invoke(this, new LocationEventArgs(location, address));
                        }

                        await Task.Delay(30000, _cancelTokenSource.Token); // 每30秒更新一次位置
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"位置更新失败: {ex.Message}");
                        await Task.Delay(5000, _cancelTokenSource.Token); // 出错时等待5秒后重试
                    }
                }
            }, _cancelTokenSource.Token);
        }

        public Task StopLocationUpdatesAsync()
        {
            _isTracking = false;
            _cancelTokenSource?.Cancel();
            return Task.CompletedTask;
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
    }
}