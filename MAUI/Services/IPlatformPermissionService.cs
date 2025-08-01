namespace LocationShareApp.Services
{
    public interface IPlatformPermissionService
    {
        Task<bool> CheckLocationPermissionsAsync();
        Task<bool> CheckBackgroundLocationPermissionAsync();
        Task<bool> RequestLocationPermissionsAsync();
        Task<bool> RequestBackgroundLocationPermissionAsync();
        Task ShowPermissionSettingsAsync();
        Task<bool> CheckBatteryOptimizationAsync();
        Task RequestBatteryOptimizationExemptionAsync();
        Task<bool> CheckBackgroundAppRefreshAsync();
        Task ShowBackgroundAppRefreshSettingsAsync();
    }
}