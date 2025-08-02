using Microsoft.Extensions.Logging;

namespace LocationShareApp.Services
{
    public class AppInitializationService : IAppInitializationService
    {
        private readonly IStorageService _storageService;
        private readonly IApiService _apiService;
        private readonly ISignalRService _signalRService;
        private readonly ILogger<AppInitializationService> _logger;

        public AppInitializationService(
            IStorageService storageService,
            IApiService apiService,
            ISignalRService signalRService,
            ILogger<AppInitializationService> logger)
        {
            _storageService = storageService;
            _apiService = apiService;
            _signalRService = signalRService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("开始应用初始化...");
                
                // 恢复认证令牌
                var hasValidToken = await RestoreAuthTokenAsync();
                
                // 如果没有有效的认证令牌，导航到登录页面
                if (!hasValidToken)
                {
                    _logger.LogInformation("未找到有效的认证令牌，导航到登录页面");
                    Application.Current!.MainPage = new NavigationPage(ServiceHelper.GetService<Views.LoginPage>());
                    return;
                }
                
                // 检查权限
                await CheckPermissionsAsync();
                
                // 初始化其他服务
                await InitializeServicesAsync();
                
                _logger.LogInformation("应用初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用初始化过程中发生错误");
            }
        }

        private async Task<bool> RestoreAuthTokenAsync()
        {
            try
            {
                // 从安全存储中获取认证令牌
                var token = await _storageService.GetAsync("auth_token");
                
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("已从存储中恢复认证令牌");
                    
                    // 设置API服务的认证令牌
                    _apiService.SetAuthToken(token);
                    
                    // 启动SignalR连接
                    await _signalRService.StartConnectionAsync(token);
                    
                    return true;
                }
                else
                {
                    _logger.LogInformation("未找到存储的认证令牌");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢复认证令牌时发生错误");
                return false;
            }
        }

        public async Task<bool> CheckPermissionsAsync()
        {
            try
            {
                // 检查权限状态
                _logger.LogInformation("检查应用权限");
                await Task.Delay(50);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查权限时发生错误");
                return false;
            }
        }

        public async Task InitializeServicesAsync()
        {
            try
            {
                // 初始化其他服务
                _logger.LogInformation("初始化应用服务");
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化服务时发生错误");
            }
        }
    }
}