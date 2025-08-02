using System.Collections.ObjectModel;
using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;
using LocationShareApp.Controls;

namespace LocationShareApp.ViewModels
{
    [QueryProperty(nameof(UserId), "userId")]
    public class UserDetailViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IDeviceInfoService _deviceInfoService;
        private int _userId;
        private ConnectedUser? _user;
        private ObservableCollection<LocationRecord> _locationHistory;
        private ObservableCollection<BatteryRecord> _batteryHistory;
        private DeviceInformation? _deviceInfo;
        private NetworkInformation? _networkInfo;

        public UserDetailViewModel(IApiService apiService, IDeviceInfoService deviceInfoService)
        {
            _apiService = apiService;
            _deviceInfoService = deviceInfoService;
            _locationHistory = new ObservableCollection<LocationRecord>();
            _batteryHistory = new ObservableCollection<BatteryRecord>();
            
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            ViewLocationHistoryCommand = new AsyncRelayCommand(ViewLocationHistoryAsync);
            ViewBatteryHistoryCommand = new AsyncRelayCommand(ViewBatteryHistoryAsync);
            ViewDeviceInfoCommand = new AsyncRelayCommand(ViewDeviceInfoAsync);
        }

        public int UserId
        {
            get => _userId;
            set
            {
                SetProperty(ref _userId, value);
                _ = LoadUserDetailAsync();
            }
        }

        public ConnectedUser? User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public ObservableCollection<LocationRecord> LocationHistory
        {
            get => _locationHistory;
            set => SetProperty(ref _locationHistory, value);
        }

        public ObservableCollection<BatteryRecord> BatteryHistory
        {
            get => _batteryHistory;
            set => SetProperty(ref _batteryHistory, value);
        }

        public DeviceInformation? DeviceInfo
        {
            get => _deviceInfo;
            set => SetProperty(ref _deviceInfo, value);
        }

        public NetworkInformation? NetworkInfo
        {
            get => _networkInfo;
            set => SetProperty(ref _networkInfo, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewLocationHistoryCommand { get; }
        public ICommand ViewBatteryHistoryCommand { get; }
        public ICommand ViewDeviceInfoCommand { get; }

        
        public async Task InitializeAsync()
        {
            await LoadUserDetailAsync();
            await LoadDeviceInfoAsync();
            await LoadLocationHistoryAsync();
            await LoadBatteryHistoryAsync();
        }
        
        private async Task LoadUserDetailAsync()
        {
            if (UserId <= 0) return;

            try
            {
                IsBusy = true;

                // 获取用户基本信息（从关联用户列表中获取）
                var connectedUsersResult = await _apiService.GetConnectedUsersAsync();
                if (connectedUsersResult.IsSuccess && connectedUsersResult.Data != null)
                {
                    User = connectedUsersResult.Data.FirstOrDefault(u => u.Id == UserId);
                    if (User != null)
                    {
                        Title = User.NickName;
                    }
                }

                // 加载24小时内的位置历史
                await LoadLocationHistoryAsync();

                // 加载24小时内的电量历史
                await LoadBatteryHistoryAsync();

                // 加载设备信息
                await LoadDeviceInfoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载用户详情失败: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadLocationHistoryAsync()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-24);

            var result = await _apiService.GetLocationHistoryAsync(UserId, startTime, endTime);
            if (result.IsSuccess && result.Data != null)
            {
                LocationHistory.Clear();
                foreach (var location in result.Data.Take(50)) // 限制显示最近50条记录
                {
                    LocationHistory.Add(location);
                }
            }
        }

        private async Task LoadBatteryHistoryAsync()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-24);

            var result = await _apiService.GetBatteryHistoryAsync(UserId, startTime, endTime);
            if (result.IsSuccess && result.Data != null)
            {
                BatteryHistory.Clear();
                foreach (var battery in result.Data)
                {
                    BatteryHistory.Add(battery);
                }
                
                // 添加调试信息
                System.Diagnostics.Debug.WriteLine($"电量历史数据加载成功，共 {BatteryHistory.Count} 条记录");
                if (BatteryHistory.Any())
                {
                    var first = BatteryHistory.First();
                    System.Diagnostics.Debug.WriteLine($"第一条记录: 电量={first.BatteryLevel}%, 充电={first.IsCharging}, 时间={first.Timestamp}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"电量历史数据加载失败: {result.ErrorMessage}");
            }
        }
        

        private async Task RefreshAsync()
        {
            await LoadUserDetailAsync();
        }

        private async Task ViewLocationHistoryAsync()
        {
            // 显示完整的位置历史
            await LoadLocationHistoryAsync();
        }

        private async Task ViewBatteryHistoryAsync()
        {
            // 显示完整的电量历史
            await LoadBatteryHistoryAsync();
        }

        private async Task LoadDeviceInfoAsync()
        {
            try
            {
                DeviceInfo = await _deviceInfoService.GetDeviceInfoAsync();
                NetworkInfo = await _deviceInfoService.GetNetworkInfoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设备信息失败: {ex.Message}");
            }
        }

        private async Task ViewDeviceInfoAsync()
        {
            await LoadDeviceInfoAsync();
        }
    }
}