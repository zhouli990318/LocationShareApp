using Microsoft.Maui.Essentials;

namespace LocationShareApp.Services;

public class AppInitializationService : IAppInitializationService
{
    private readonly IStorageService _storageService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlatformPermissionService _permissionService;
    
    public event EventHandler<InitializationEventArgs>? InitializationProgress;

    public AppInitializationService(
        IStorageService storageService,
        IConfigurationService configurationService,
        IPlatformPermissionService permissionService)
    {
        _storageService = storageService;
        _configurationService = configurationService;
        _permissionService = permissionService;
    }

    public async Task<bool> InitializeAppAsync()
    {
        try
        {
            var result = await GetInitializationStatusAsync();
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"应用初始化失败: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CheckAndRequestPermissionsAsync()
    {
        try
        {
            OnInitializationProgress("检查权限", 20);
            
            // 检查位置权限
            var locationStatus = await _permissionService.CheckLocationPermissionAsync();
            if (!locationStatus)
            {
                locationStatus = await _permissionService.RequestLocationPermissionAsync();
            }

            // 检查电池优化权限
            var batteryStatus = await _permissionService.CheckBatteryOptimizationPermissionAsync();
            
            OnInitializationProgress("权限检查完成", 40, locationStatus && batteryStatus);
            return locationStatus && batteryStatus;
        }
        catch (Exception ex)
        {
            OnInitializationProgress("权限检查失败", 40, false, ex.Message);
            return false;
        }
    }

    public async Task<bool> ValidateAppConfigurationAsync()
    {
        try
        {
            OnInitializationProgress("验证配置", 60);
            
            var config = await _configurationService.GetConfigurationAsync();
            var isValid = !string.IsNullOrEmpty(config.ApiBaseUrl) && 
                         !string.IsNullOrEmpty(config.SignalRHubUrl);
            
            OnInitializationProgress("配置验证完成", 80, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            OnInitializationProgress("配置验证失败", 80, false, ex.Message);
            return false;
        }
    }

    public async Task<InitializationResult> GetInitializationStatusAsync()
    {
        var result = new InitializationResult();
        
        try
        {
            OnInitializationProgress("开始初始化", 0);
            
            // 步骤1: 检查权限
            var permissionResult = await CheckAndRequestPermissionsAsync();
            if (permissionResult)
            {
                result.CompletedSteps.Add("权限检查");
            }
            else
            {
                result.FailedSteps.Add("权限检查");
                result.Warnings.Add("部分权限未授予，可能影响应用功能");
            }

            // 步骤2: 验证配置
            var configResult = await ValidateAppConfigurationAsync();
            if (configResult)
            {
                result.CompletedSteps.Add("配置验证");
            }
            else
            {
                result.FailedSteps.Add("配置验证");
                result.ErrorMessage = "应用配置无效";
            }

            // 步骤3: 初始化服务
            OnInitializationProgress("初始化服务", 90);
            await InitializeServicesAsync();
            result.CompletedSteps.Add("服务初始化");

            result.IsSuccess = result.FailedSteps.Count == 0;
            OnInitializationProgress("初始化完成", 100, result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.FailedSteps.Add("初始化异常");
            OnInitializationProgress("初始化失败", 100, false, ex.Message);
            return result;
        }
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            // 初始化存储服务
            await _storageService.InitializeAsync();
            
            // 预加载配置
            await _configurationService.LoadConfigurationAsync();
            
            System.Diagnostics.Debug.WriteLine("服务初始化完成");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"服务初始化失败: {ex.Message}");
            throw;
        }
    }

    private void OnInitializationProgress(string step, int progress, bool isSuccess = true, string? message = null)
    {
        InitializationProgress?.Invoke(this, new InitializationEventArgs
        {
            Step = step,
            Progress = progress,
            IsSuccess = isSuccess,
            Message = message
        });
    }
}