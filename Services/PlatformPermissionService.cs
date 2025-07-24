#if ANDROID
using LocationShareApp.Platforms.Android.Services;
#elif IOS
using LocationShareApp.Platforms.iOS.Services;
#endif

namespace LocationShareApp.Services
{
    public class PlatformPermissionService : IPlatformPermissionService
    {
        public async Task<bool> CheckLocationPermissionsAsync()
        {
            try
            {
#if ANDROID
                return AndroidSpecificService.CheckLocationPermissions();
#elif IOS
                return iOSSpecificService.CheckLocationPermissions();
#else
                // 其他平台的默认实现
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查位置权限失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckBackgroundLocationPermissionAsync()
        {
            try
            {
#if ANDROID
                return AndroidSpecificService.CheckBackgroundLocationPermission();
#elif IOS
                return iOSSpecificService.CheckBackgroundLocationPermission();
#else
                // 其他平台的默认实现
                return await CheckLocationPermissionsAsync();
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查后台位置权限失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestLocationPermissionsAsync()
        {
            try
            {
#if ANDROID
                AndroidSpecificService.RequestLocationPermissions();
                // 等待用户操作
                await Task.Delay(1000);
                return AndroidSpecificService.CheckLocationPermissions();
#elif IOS
                iOSSpecificService.RequestLocationPermissions();
                // 等待用户操作
                await Task.Delay(1000);
                return iOSSpecificService.CheckLocationPermissions();
#else
                // 其他平台的默认实现
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"请求位置权限失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestBackgroundLocationPermissionAsync()
        {
            try
            {
#if ANDROID
                AndroidSpecificService.RequestBackgroundLocationPermission();
                // 等待用户操作
                await Task.Delay(1000);
                return AndroidSpecificService.CheckBackgroundLocationPermission();
#elif IOS
                iOSSpecificService.RequestBackgroundLocationPermission();
                // 等待用户操作
                await Task.Delay(1000);
                return iOSSpecificService.CheckBackgroundLocationPermission();
#else
                // 其他平台的默认实现
                return await RequestLocationPermissionsAsync();
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"请求后台位置权限失败: {ex.Message}");
                return false;
            }
        }

        public async Task ShowPermissionSettingsAsync()
        {
            try
            {
#if ANDROID
                AndroidSpecificService.OpenAppSettings();
#elif IOS
                iOSSpecificService.OpenAppSettings();
#else
                // 其他平台的默认实现
                await Launcher.OpenAsync("app-settings:");
#endif
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开权限设置失败: {ex.Message}");
            }
        }

        public async Task<bool> CheckBatteryOptimizationAsync()
        {
            try
            {
#if ANDROID
                return AndroidSpecificService.IsBatteryOptimizationIgnored();
#elif IOS
                // iOS不需要电池优化设置
                return true;
#else
                // 其他平台的默认实现
                return true;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查电池优化失败: {ex.Message}");
                return true;
            }
        }

        public async Task RequestBatteryOptimizationExemptionAsync()
        {
            try
            {
#if ANDROID
                AndroidSpecificService.RequestIgnoreBatteryOptimization();
#elif IOS
                // iOS不需要电池优化设置
#endif
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"请求电池优化豁免失败: {ex.Message}");
            }
        }

        public async Task<bool> CheckBackgroundAppRefreshAsync()
        {
            try
            {
#if ANDROID
                // Android通过电池优化来控制后台运行
                return AndroidSpecificService.IsBatteryOptimizationIgnored();
#elif IOS
                return iOSSpecificService.IsBackgroundAppRefreshEnabled();
#else
                // 其他平台的默认实现
                return true;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查后台应用刷新失败: {ex.Message}");
                return true;
            }
        }

        public async Task ShowBackgroundAppRefreshSettingsAsync()
        {
            try
            {
#if ANDROID
                AndroidSpecificService.RequestIgnoreBatteryOptimization();
#elif IOS
                iOSSpecificService.ShowBackgroundAppRefreshAlert();
#endif
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示后台应用刷新设置失败: {ex.Message}");
            }
        }
    }
}