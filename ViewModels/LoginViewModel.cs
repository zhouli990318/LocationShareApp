using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace LocationShareApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IStorageService _storageService;
        private readonly ISignalRService _signalRService;

        private string _phoneNumber = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(IApiService apiService, IStorageService storageService, ISignalRService signalRService)
        {
            _apiService = apiService;
            _storageService = storageService;
            _signalRService = signalRService;
            
            Title = "登录";
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            GoToRegisterCommand = new AsyncRelayCommand(GoToRegisterAsync);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                SetProperty(ref _phoneNumber, value);
                ((AsyncRelayCommand)LoginCommand).NotifyCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ((AsyncRelayCommand)LoginCommand).NotifyCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(PhoneNumber) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsBusy;
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.LoginAsync(PhoneNumber, Password);
                
                if (result.IsSuccess && result.Data != null)
                {
                    // 保存登录信息
                    await _storageService.SetAsync("auth_token", result.Data.Token);
                    await _storageService.SetAsync("user_info", System.Text.Json.JsonSerializer.Serialize(result.Data.User));
                    
                    // 设置API认证令牌
                    _apiService.SetAuthToken(result.Data.Token);
                    
                    // 启动SignalR连接
                    await _signalRService.StartConnectionAsync(result.Data.Token);
                    
                    // 导航到主页面
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "登录失败";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"登录失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}