using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace LocationShareApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IStorageService _storageService;
        private readonly ISignalRService _signalRService;
        private readonly ILogger<LoginViewModel> _logger;

        private string _phoneNumber = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(IApiService apiService, IStorageService storageService, ISignalRService signalRService, ILogger<LoginViewModel> logger)
        {
            _apiService = apiService;
            _storageService = storageService;
            _signalRService = signalRService;
            _logger = logger;
            
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
                    
                    // 切换到AppShell并导航到主页面
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "登录失败";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录过程中发生异常。用户: {PhoneNumber}, 错误信息: {ErrorMessage}", PhoneNumber, ex.Message);
                ErrorMessage = $"登录失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToRegisterAsync()
        {
            try
            {
                // 如果当前不在Shell中，需要先获取导航服务
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("register");
                }
                else
                {
                    // 当前在NavigationPage中，使用传统导航
                    var registerPage = ServiceHelper.GetService<Views.RegisterPage>();
                    if (registerPage != null)
                    {
                        await Application.Current!.MainPage.Navigation.PushAsync(registerPage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导航到注册页面时发生异常: {ErrorMessage}", ex.Message);
                ErrorMessage = $"导航失败: {ex.Message}";
            }
        }
    }
}