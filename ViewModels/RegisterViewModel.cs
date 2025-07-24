using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace LocationShareApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IStorageService _storageService;
        private readonly ISignalRService _signalRService;

        private string _phoneNumber = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _nickName = string.Empty;
        private string _errorMessage = string.Empty;

        public RegisterViewModel(IApiService apiService, IStorageService storageService, ISignalRService signalRService)
        {
            _apiService = apiService;
            _storageService = storageService;
            _signalRService = signalRService;
            
            Title = "注册";
            RegisterCommand = new AsyncRelayCommand(RegisterAsync, CanRegister);
            GoToLoginCommand = new AsyncRelayCommand(GoToLoginAsync);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                SetProperty(ref _phoneNumber, value);
                ((AsyncRelayCommand)RegisterCommand).NotifyCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ((AsyncRelayCommand)RegisterCommand).NotifyCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ((AsyncRelayCommand)RegisterCommand).NotifyCanExecuteChanged();
            }
        }

        public string NickName
        {
            get => _nickName;
            set
            {
                SetProperty(ref _nickName, value);
                ((AsyncRelayCommand)RegisterCommand).NotifyCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(PhoneNumber) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(NickName) &&
                   Password == ConfirmPassword &&
                   !IsBusy;
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "两次输入的密码不一致";
                    return;
                }

                var result = await _apiService.RegisterAsync(PhoneNumber, Password, NickName);
                
                if (result.IsSuccess && result.Data != null)
                {
                    // 保存登录信息
                    await _storageService.SetAsync("auth_token", result.Data.Token);
                    await _storageService.SetAsync("user_info", System.Text.Json.JsonSerializer.Serialize(result.Data.User));
                    
                    // 设置API认证令牌
                    _apiService.SetAuthToken(result.Data.Token);
                    
                    // 启动SignalR连接
                    await _signalRService.StartConnectionAsync(result.Data.Token);
                    
                    // 显示绑定码
                    await Application.Current!.MainPage!.DisplayAlert(
                        "注册成功", 
                        $"您的绑定码是: {result.Data.User.BindingCode}\n请妥善保管，其他用户可通过此码关联您", 
                        "确定");
                    
                    // 导航到主页面
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "注册失败";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"注册失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}