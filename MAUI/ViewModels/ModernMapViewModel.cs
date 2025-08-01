using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationShareApp.Models;
using LocationShareApp.Services;
using LocationShareApp.Commands;

namespace LocationShareApp.ViewModels
{
    public class ModernMapViewModel : INotifyPropertyChanged
    {
        private readonly ILocationService _locationService;
        private readonly IMauiMapService _mapService;
        private bool _isBusy;
        private string _searchText = string.Empty;
        private MapType _currentMapType = MapType.Street;
        private Location? _currentLocation;
        private MapBounds? _mapBounds;

        public ModernMapViewModel(ILocationService locationService, IMauiMapService mapService)
        {
            _locationService = locationService;
            _mapService = mapService;
            
            Markers = new ObservableCollection<MapMarker>();
            ConnectedUsers = new ObservableCollection<UserLocation>();
            MapMarkers = new ObservableCollection<MapMarker>();
            
            // 初始化命令
            GoToCurrentLocationCommand = new AsyncCommand(GoToCurrentLocation);
            AddMarkerCommand = new AsyncCommand<Location>(AddMarker);
            RemoveMarkerCommand = new AsyncCommand<MapMarker>(RemoveMarker);
            SearchLocationCommand = new AsyncCommand(SearchLocation);
            ToggleMapTypeCommand = new Command(ToggleMapType);
            ShareLocationCommand = new AsyncCommand(ShareLocation);
            
            // 订阅位置变化事件
            _locationService.LocationChanged += OnLocationChanged;
            
            // 初始化地图
            _ = InitializeMapAsync();
        }

        #region Properties

        public ObservableCollection<MapMarker> Markers { get; }
        public ObservableCollection<UserLocation> ConnectedUsers { get; }
        public ObservableCollection<MapMarker> MapMarkers { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public Location? CurrentLocation
        {
            get => _currentLocation;
            set => SetProperty(ref _currentLocation, value);
        }

        public MapBounds? MapBounds
        {
            get => _mapBounds;
            set => SetProperty(ref _mapBounds, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public MapType CurrentMapType
        {
            get => _currentMapType;
            set => SetProperty(ref _currentMapType, value);
        }

        #endregion

        #region Commands

        public AsyncCommand GoToCurrentLocationCommand { get; }
        public AsyncCommand<Location> AddMarkerCommand { get; }
        public AsyncCommand<MapMarker> RemoveMarkerCommand { get; }
        public AsyncCommand SearchLocationCommand { get; }
        public ICommand ToggleMapTypeCommand { get; }
        public AsyncCommand ShareLocationCommand { get; }

        #endregion

        #region Public Methods

        public async Task InitializeMapAsync()
        {
            try
            {
                IsBusy = true;
                
                // 请求位置权限
                var hasPermission = await _locationService.RequestLocationPermissionAsync();
                if (!hasPermission)
                {
                    await Application.Current.MainPage.DisplayAlert("权限需要", "需要位置权限才能使用地图功能", "确定");
                    return;
                }

                // 获取当前位置
                CurrentLocation = await _locationService.GetCurrentLocationAsync();
                
                if (CurrentLocation != null)
                {
                    // 初始化地图中心点
                    MapBounds = new MapBounds
                    {
                        NorthLatitude = CurrentLocation.Latitude + 0.01,
                        SouthLatitude = CurrentLocation.Latitude - 0.01,
                        EastLongitude = CurrentLocation.Longitude + 0.01,
                        WestLongitude = CurrentLocation.Longitude - 0.01
                    };
                    
                    // 添加当前位置标记
                    var currentMarker = new MapMarker
                    {
                        Id = "current_location",
                        Title = "我的位置",
                        Description = "当前位置",
                        Latitude = CurrentLocation.Latitude,
                        Longitude = CurrentLocation.Longitude,
                        MarkerType = MapMarkerType.CurrentLocation
                    };
                    
                    Markers.Add(currentMarker);
                }
                
                // 开始位置更新
                await _locationService.StartLocationUpdatesAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"初始化地图失败: {ex.Message}", "确定");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Private Methods

        private async Task GoToCurrentLocation()
        {
            try
            {
                IsBusy = true;
                
                CurrentLocation = await _locationService.GetCurrentLocationAsync();
                
                if (CurrentLocation != null)
                {
                    MapBounds = new MapBounds
                    {
                        NorthLatitude = CurrentLocation.Latitude + 0.005,
                        SouthLatitude = CurrentLocation.Latitude - 0.005,
                        EastLongitude = CurrentLocation.Longitude + 0.005,
                        WestLongitude = CurrentLocation.Longitude - 0.005
                    };
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"获取当前位置失败: {ex.Message}", "确定");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddMarker(Location location)
        {
            try
            {
                var marker = new MapMarker
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"标记点 {DateTime.Now:HH:mm}",
                    Description = "用户添加的标记",
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    MarkerType = MapMarkerType.Custom,
                    Timestamp = DateTime.Now
                };
                
                Markers.Add(marker);
                
                // 通知地图服务添加标记
                var mapPin = new Services.MapPin
                {
                    Id = marker.Id,
                    Title = marker.Title,
                    Description = marker.Description,
                    Latitude = marker.Latitude,
                    Longitude = marker.Longitude
                };
                
                await _mapService.AddPinAsync(mapPin);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"添加标记失败: {ex.Message}", "确定");
            }
        }

        private async Task RemoveMarker(MapMarker marker)
        {
            try
            {
                Markers.Remove(marker);
                
                // 通知地图服务移除标记
                var mapPin = new Services.MapPin
                {
                    Id = marker.Id,
                    Latitude = marker.Latitude,
                    Longitude = marker.Longitude
                };
                
                await _mapService.RemovePinAsync(mapPin);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"删除标记失败: {ex.Message}", "确定");
            }
        }

        private async Task SearchLocation()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            try
            {
                IsBusy = true;
                
                // 这里可以集成地理编码服务来搜索位置
                // 暂时使用简单的实现
                await Application.Current.MainPage.DisplayAlert("搜索", $"搜索位置: {SearchText}", "确定");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ToggleMapType()
        {
            // MAUI Maps 支持的地图类型切换
            CurrentMapType = CurrentMapType switch
            {
                MapType.Street => MapType.Satellite,
                MapType.Satellite => MapType.Hybrid,
                MapType.Hybrid => MapType.Street,
                _ => MapType.Street
            };
        }

        private async Task ShareLocation()
        {
            try
            {
                if (CurrentLocation == null)
                {
                    CurrentLocation = await _locationService.GetCurrentLocationAsync();
                }

                if (CurrentLocation != null)
                {
                    var shareText = $"我的位置: {CurrentLocation.Latitude:F6}, {CurrentLocation.Longitude:F6}";
                    await Share.RequestAsync(new ShareTextRequest
                    {
                        Text = shareText,
                        Title = "分享我的位置"
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("错误", "无法获取当前位置", "确定");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"分享位置失败: {ex.Message}", "确定");
            }
        }

        #endregion

        #region Event Handlers

        private void OnLocationChanged(object? sender, LocationEventArgs e)
        {
            CurrentLocation = e.Location;
            
            // 更新当前位置标记
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var currentMarker = Markers.FirstOrDefault(m => m.Id == "current_location");
                if (currentMarker != null)
                {
                    currentMarker.Latitude = e.Location.Latitude;
                    currentMarker.Longitude = e.Location.Longitude;
                    currentMarker.Timestamp = DateTime.Now;
                }
            });
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = "", Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            _locationService.LocationChanged -= OnLocationChanged;
        }

        #endregion
    }

    // 用户位置模型
    public class UserLocation
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Location? Location { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}