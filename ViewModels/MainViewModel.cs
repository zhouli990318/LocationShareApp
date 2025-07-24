using System.Collections.ObjectModel;
using System.Windows.Input;
using LocationShareApp.Services;
using LocationShareApp.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

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

        private UserProfile? _currentUser;
        private Location? _currentLocation;
        private string _syncStatus = "未同步";

        public MainViewModel(
            IApiService apiService, 
            ILocationService locationService, 
            IBatteryService batteryService,
            ISignalRService signalRService,
            IStorageService storageService,
            IBackgroundSyncService backgroundSyncService,
            IDataCacheService dataCacheService)
        {
            _apiService = apiService;
            _locationService = locationService;
            _batteryService = batteryService;
            _signalRService = signalRService;
            _storageService = storageService;
            _backgroundSyncService = backgroundSyncService;
            _dataCacheService = dataCacheService;

            Title = "位置共享";
            ConnectedUsers = new ObservableCollection<ConnectedUser>();
            
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            UserTappedCommand = new AsyncRelayCommand<ConnectedUser>(UserTappedAsync);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

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

        public ICommand RefreshCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand UserTappedCommand { get; }
        public ICommand LogoutCommand { get; }

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
                System.Diagnostics.Debug.WriteLine($"初始化失败: {ex.Message}");
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
                Application.Current!.MainPage = new NavigationPage(ServiceHelper.GetService<LoginPage>());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"登出失败: {ex.Message}");
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
    }
}