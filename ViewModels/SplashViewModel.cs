using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace LocationShareApp.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IAppInitializationService _initializationService;
        private readonly IStorageService _storageService;
        private readonly IApiService _apiService;
        private readonly ISignalRService _signalRService;

        private double _initializationProgress;
        private string _currentStep = "正在启动应用...";
        private bool _isInitializing = true;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private string _appVersion = string.Empty;

        public SplashViewModel(
            IAppInitializationService initializationService,
            IStorageService storageService,
            IApiService apiService,
            ISignalRService signalRService)
        {
            _initializationService = initializationService;
            _storageService = storageService;
            _apiService = apiService;
            _signalRService = signalRService;

            RetryCommand = new AsyncRelayCommand(InitializeAsync);
            
            // 获取应用版本
            AppVersion = AppInfo.Current.VersionString;
            
            // 订阅初始化进度事件
            _initializationService.InitializationProgress += OnInitializationProgress;
        }

        public double InitializationProgress
        {
            get => _initializationProgress;
            set => SetProperty(ref _initializationProgress, value);
        }

        public string CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            set => SetProperty(ref _isInitializing, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }

        public ICommand RetryCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                IsInitializing = true;
                HasError = false;
                ErrorMessage = string.Empty;
                InitializationProgress = 0;
                CurrentStep = "正在启动应用...";

                // 执行应用初始化
                var success = await _initializationService.InitializeAppAsync();
                
                if (success)
                {
                    // 检查是否已登录
                    await CheckLoginStatusAsync();
                }
                else
                {
                    // 初始化失败，显示错误信息
                    var result = await _initializationService.GetInitializationStatusAsync();
                    ShowError(result.ErrorMessage ?? "应用初始化失败");
                }
            }
            catch (Exception ex)
            {
                ShowError($"应用启动异常: {ex.Message}");
            }
            finally
            {
                IsInitializing = false;
            }
        }

        private async Task CheckLoginStatusAsync()
        {
            try
            {
                CurrentStep = "检查登录状态...";
                
                var token = await _storageService.GetAsync("auth_token");
                
                if (!string.IsNullOrEmpty(token))
                {
                    // 已登录，设置API认证令牌
                    _apiService.SetAuthToken(token);
                    
                    // 启动SignalR连接
                    await _signalRService.StartConnectionAsync(token);
                    
                    // 导航到主页面
                    CurrentStep = "登录成功，正在进入应用...";
                    await Task.Delay(500); // 短暂延迟以显示消息
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    // 未登录，显示登录页面
                    CurrentStep = "正在进入登录页面...";
                    await Task.Delay(500);
                    Application.Current!.MainPage = new NavigationPage(ServiceHelper.GetService<Views.LoginPage>());
                }
            }
            catch (Exception ex)
            {
                ShowError($"检查登录状态失败: {ex.Message}");
            }
        }

        private void OnInitializationProgress(object? sender, InitializationEventArgs e)
        {
            CurrentStep = e.Step;
            InitializationProgress = e.Progress / 100.0;
            
            if (!e.IsSuccess && !string.IsNullOrEmpty(e.Message))
            {
                ShowError(e.Message);
            }
        }

        private void ShowError(string message)
        {
            HasError = true;
            ErrorMessage = message;
            IsInitializing = false;
        }
    }
}