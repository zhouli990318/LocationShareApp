using CommunityToolkit.Mvvm.Input;
using LocationShareApp.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LocationShareApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ILocationService _locationService;
        private readonly IBatteryService _batteryService;
        private readonly ISignalRService _signalRService;
        private readonly IStorageService _storageService;
        private readonly IBackgroundSyncService _backgroundSyncService;
        private readonly IDataCacheService _dataCacheService;
        private readonly ILogger<MainViewModel> _logger;

        private UserProfile? _currentUser;
        private Location? _currentLocation;
        private string _syncStatus = "未同步";
        private ObservableCollection<MapPin> _mapPins;

        public MainViewModel(
            IApiService apiService,
            ILocationService locationService,
            IBatteryService batteryService,
            ISignalRService signalRService,
            IStorageService storageService,
            IBackgroundSyncService backgroundSyncService,
            IDataCacheService dataCacheService,
            ILogger<MainViewModel> logger)
        {
            _apiService = apiService;
            _locationService = locationService;
            _batteryService = batteryService;
            _signalRService = signalRService;
            _storageService = storageService;
            _backgroundSyncService = backgroundSyncService;
            _dataCacheService = dataCacheService;
            _logger = logger;

            Title = "位置共享";
            ConnectedUsers = new ObservableCollection<ConnectedUser>();
            _mapPins = new ObservableCollection<MapPin>();

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            UserTappedCommand = new AsyncRelayCommand<ConnectedUser>(UserTappedAsync);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
            LocationCommand = new AsyncRelayCommand(LocationCommandAsync);
            LayerCommand = new AsyncRelayCommand(LayerCommandAsync);
            FriendsCommand = new AsyncRelayCommand(FriendsCommandAsync);

            // 订阅SignalR事件
            _signalRService.UserLocationUpdated += OnUserLocationUpdated;
            _signalRService.UserBatteryUpdated += OnUserBatteryUpdated;
            _signalRService.UserOnline += OnUserOnline;
            _signalRService.UserOffline += OnUserOffline;

            // 订阅位置和电量变化事件
            _locationService.LocationChanged += OnLocationChanged;
            _batteryService.BatteryChanged += OnBatteryChanged;

            // 订阅后台同步状态变化事件
            _backgroundSyncService.SyncStatusChanged += OnSyncStatusChanged;
        }

        public UserProfile? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public Location? CurrentLocation
        {
            get => _currentLocation;
            set => SetProperty(ref _currentLocation, value);
        }

        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
        }

        public ObservableCollection<ConnectedUser> ConnectedUsers { get; }

        public ObservableCollection<MapPin> MapPins
        {
            get => _mapPins;
            set => SetProperty(ref _mapPins, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand UserTappedCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LocationCommand { get; }
        public ICommand LayerCommand { get; }
        public ICommand FriendsCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;

                // 加载用户信息
                await LoadUserProfileAsync();

                // 加载关联用户
                await LoadConnectedUsersAsync();

                // 启动位置和电量监控
                await _locationService.StartLocationUpdatesAsync();
                await _batteryService.StartBatteryMonitoringAsync();

                // 启动后台同步服务
                await _backgroundSyncService.StartSyncAsync();

                // 同步缓存的数据
                await _dataCacheService.SyncCachedDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MainViewModel初始化失败");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadUserProfileAsync()
        {
            var result = await _apiService.GetProfileAsync();
            if (result.IsSuccess && result.Data != null)
            {
                CurrentUser = result.Data;
            }
        }

        private async Task LoadConnectedUsersAsync()
        {
            var result = await _apiService.GetConnectedUsersAsync();
            if (result.IsSuccess && result.Data != null)
            {
                ConnectedUsers.Clear();
                foreach (var user in result.Data)
                {
                    ConnectedUsers.Add(user);
                }
            }
        }

        private async Task RefreshAsync()
        {
            await LoadConnectedUsersAsync();
        }

        private async Task AddUserAsync()
        {
            await Shell.Current.GoToAsync("usermanagement");
        }

        private async Task UserTappedAsync(ConnectedUser? user)
        {
            if (user != null)
            {
                await Shell.Current.GoToAsync($"userdetail?userId={user.Id}");
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                // 停止后台同步服务
                await _backgroundSyncService.StopSyncAsync();

                // 停止位置和电量监控
                await _locationService.StopLocationUpdatesAsync();
                await _batteryService.StopBatteryMonitoringAsync();

                // 停止SignalR连接
                await _signalRService.StopConnectionAsync();

                // 清除存储的认证信息
                await _storageService.ClearAsync();
                _apiService.ClearAuthToken();

                // 导航到登录页面
                Application.Current!.MainPage = new NavigationPage(ServiceHelper.GetService<Views.LoginPage>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登出失败");
            }
        }

        private async void OnLocationChanged(object? sender, LocationEventArgs e)
        {
            CurrentLocation = e.Location;

            // 缓存位置数据
            var cacheItem = new LocationCacheItem
            {
                Latitude = e.Location.Latitude,
                Longitude = e.Location.Longitude,
                Address = e.Address,
                Accuracy = e.Location.Accuracy ?? 0,
                Timestamp = DateTime.Now,
                IsSynced = false
            };
            await _dataCacheService.CacheLocationDataAsync(cacheItem);
        }

        private async void OnBatteryChanged(object? sender, BatteryEventArgs e)
        {
            // 缓存电量数据
            var cacheItem = new BatteryCacheItem
            {
                BatteryLevel = (int)(e.BatteryLevel * 100),
                IsCharging = e.IsCharging,
                Timestamp = DateTime.Now,
                IsSynced = false
            };
            await _dataCacheService.CacheBatteryDataAsync(cacheItem);
        }

        private void OnSyncStatusChanged(object? sender, SyncStatusEventArgs e)
        {
            SyncStatus = e.Status;
        }

        private void OnUserLocationUpdated(object? sender, UserLocationUpdatedEventArgs e)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.Id == e.UserId);
            if (user != null)
            {
                user.Location = new LocationInfo
                {
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    Address = e.Address,
                    Timestamp = e.Timestamp
                };
            }
        }

        private void OnUserBatteryUpdated(object? sender, UserBatteryUpdatedEventArgs e)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.Id == e.UserId);
            if (user != null)
            {
                user.Battery = new BatteryInfo
                {
                    Level = e.BatteryLevel,
                    IsCharging = e.IsCharging,
                    Timestamp = e.Timestamp
                };
            }
        }

        private void OnUserOnline(object? sender, UserOnlineEventArgs e)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.Id == e.UserId);
            if (user != null)
            {
                user.IsOnline = true;
            }
        }

        private void OnUserOffline(object? sender, UserOfflineEventArgs e)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.Id == e.UserId);
            if (user != null)
            {
                user.IsOnline = false;
            }
        }

        // 地图相关方法
        public void UpdateMapPins(List<MapPin> pins)
        {
            MapPins.Clear();
            foreach (var pin in pins)
            {
                MapPins.Add(pin);
            }
        }

        public void AddMapPin(MapPin pin)
        {
            MapPins.Add(pin);
        }

        public void RemoveMapPin(MapPin pin)
        {
            MapPins.Remove(pin);
        }

        private async Task LocationCommandAsync()
        {
            try
            {
                // 定位到当前位置
                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    CurrentLocation = location;
                    // 这里可以触发地图移动到当前位置的事件
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前位置失败");
            }
        }

        private async Task LayerCommandAsync()
        {
            try
            {
                // 切换地图图层
                // 这里可以实现地图样式切换逻辑
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换地图图层失败");
            }
        }

        private async Task FriendsCommandAsync()
        {
            try
            {
                // 显示好友列表
                await Shell.Current.GoToAsync("usermanagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "显示好友列表失败");
            }
        }
    }
}
