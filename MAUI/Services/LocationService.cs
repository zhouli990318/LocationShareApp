using LocationShareApp.Models;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace LocationShareApp.Services
{
    public class LocationService : ILocationService
    {
        private bool _isTracking;
        private CancellationTokenSource? _cancelTokenSource;
        private readonly ObservableCollection<MapMarker> _mapMarkers;
        private readonly ILogger<LocationService> _logger;

        public event EventHandler<LocationEventArgs>? LocationChanged;
        public event EventHandler<MapMarker>? MarkerAdded;
        public event EventHandler<MapMarker>? MarkerRemoved;
        public event EventHandler<MapMarker>? MarkerUpdated;

        public LocationService(ILogger<LocationService> logger)
        {
            _mapMarkers = new ObservableCollection<MapMarker>();
            _logger = logger;
        }

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
                _logger.LogError(ex, "获取当前位置失败");
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
                _logger.LogError(ex, "请求位置权限失败");
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
                        _logger.LogError(ex, "位置更新失败");
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
                _logger.LogError(ex, "获取地址失败");
            }

            return $"{location.Latitude:F6}, {location.Longitude:F6}";
        }

        // 地图相关功能
        public async Task<MapMarker> AddUserLocationMarkerAsync(UserLocation userLocation)
        {
            var marker = new MapMarker
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userLocation.UserId,
                //UserName = userLocation.UserName,
                Latitude = userLocation.Latitude,
                Longitude = userLocation.Longitude,
                Address = userLocation.Address,
                Timestamp = userLocation.Timestamp,
                MarkerType = MapMarkerType.UserLocation,
                IsVisible = true
            };

            _mapMarkers.Add(marker);
            MarkerAdded?.Invoke(this, marker);

            return marker;
        }

        public async Task UpdateUserLocationMarkerAsync(int userId, Location location)
        {
            var existingMarker = _mapMarkers.FirstOrDefault(m => m.UserId == userId);
            if (existingMarker != null)
            {
                existingMarker.Latitude = location.Latitude;
                existingMarker.Longitude = location.Longitude;
                existingMarker.Address = await GetAddressFromLocationAsync(location);
                existingMarker.Timestamp = DateTime.Now;

                MarkerUpdated?.Invoke(this, existingMarker);
            }
        }

        public Task RemoveUserLocationMarkerAsync(int userId)
        {
            var marker = _mapMarkers.FirstOrDefault(m => m.UserId == userId);
            if (marker != null)
            {
                _mapMarkers.Remove(marker);
                MarkerRemoved?.Invoke(this, marker);
            }

            return Task.CompletedTask;
        }

        public async Task<MapMarker> AddCustomMarkerAsync(double latitude, double longitude, string title, string description = "")
        {
            var address = await GetAddressFromLocationAsync(new Location(latitude, longitude));

            var marker = new MapMarker
            {
                Id = Guid.NewGuid().ToString(),
                Latitude = latitude,
                Longitude = longitude,
                Title = title,
                Description = description,
                Address = address,
                Timestamp = DateTime.Now,
                MarkerType = MapMarkerType.Custom,
                IsVisible = true
            };

            _mapMarkers.Add(marker);
            MarkerAdded?.Invoke(this, marker);

            return marker;
        }

        public Task RemoveMarkerAsync(string markerId)
        {
            var marker = _mapMarkers.FirstOrDefault(m => m.Id == markerId);
            if (marker != null)
            {
                _mapMarkers.Remove(marker);
                MarkerRemoved?.Invoke(this, marker);
            }

            return Task.CompletedTask;
        }

        public IEnumerable<MapMarker> GetAllMarkers()
        {
            return _mapMarkers.ToList();
        }

        public IEnumerable<MapMarker> GetUserLocationMarkers()
        {
            return _mapMarkers.Where(m => m.MarkerType == MapMarkerType.UserLocation).ToList();
        }

        public async Task<double> CalculateDistanceAsync(Location from, Location to)
        {
            return Location.CalculateDistance(from, to, DistanceUnits.Kilometers);
        }

        public async Task<MapBounds> GetOptimalMapBoundsAsync()
        {
            if (!_mapMarkers.Any())
            {
                // 如果没有标记点，返回默认的地图边界（以当前位置为中心）
                var currentLocation = await GetCurrentLocationAsync();
                if (currentLocation != null)
                {
                    return new MapBounds
                    {
                        NorthLatitude = currentLocation.Latitude + 0.01,
                        SouthLatitude = currentLocation.Latitude - 0.01,
                        EastLongitude = currentLocation.Longitude + 0.01,
                        WestLongitude = currentLocation.Longitude - 0.01
                    };
                }

                // 默认显示中国地图范围
                return new MapBounds
                {
                    NorthLatitude = 53.5,
                    SouthLatitude = 18.2,
                    EastLongitude = 134.8,
                    WestLongitude = 73.5
                };
            }

            var latitudes = _mapMarkers.Select(m => m.Latitude).ToList();
            var longitudes = _mapMarkers.Select(m => m.Longitude).ToList();

            var padding = 0.005; // 添加一些边距

            return new MapBounds
            {
                NorthLatitude = latitudes.Max() + padding,
                SouthLatitude = latitudes.Min() - padding,
                EastLongitude = longitudes.Max() + padding,
                WestLongitude = longitudes.Min() - padding
            };
        }

    }
}
