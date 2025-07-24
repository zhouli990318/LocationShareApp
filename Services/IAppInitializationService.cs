namespace LocationShareApp.Services;

/// <summary>
/// 应用初始化服务接口
/// </summary>
public interface IAppInitializationService
{
    /// <summary>
    /// 异步初始化应用
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync();
    
    /// <summary>
    /// 检查权限状态
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckPermissionsAsync();
    
    /// <summary>
    /// 初始化服务
    /// </summary>
    /// <returns></returns>
    Task InitializeServicesAsync();
}