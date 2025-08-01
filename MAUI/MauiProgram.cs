using System.Text.Json;
using LocationShareApp.Services;
using LocationShareApp.ViewModels;
using LocationShareApp.Views;
using Microsoft.Extensions.Logging;

namespace LocationShareApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 注册服务
            builder.Services.AddSingleton<IAppInitializationService, AppInitializationService>();
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<IBatteryService, BatteryService>();
            builder.Services.AddSingleton<ISignalRService, SignalRService>();
            builder.Services.AddSingleton<IStorageService, StorageService>();
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            builder.Services.AddSingleton<IPlatformPermissionService, PlatformPermissionService>();
            builder.Services.AddSingleton<IDeviceInfoService, DeviceInfoService>();
            builder.Services.AddSingleton<IBackgroundSyncService, BackgroundSyncService>();
            builder.Services.AddSingleton<IDataCacheService, DataCacheService>();
            builder.Services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
            builder.Services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
            builder.Services.AddSingleton<IMauiMapService, MauiMapService>();

            // 注册ViewModels
            // 注册ViewModels
            builder.Services.AddTransient<SplashViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<UserDetailViewModel>();
            builder.Services.AddTransient<UserManagementViewModel>();
            builder.Services.AddTransient<ModernMapViewModel>();

            // 注册Views
            // 注册Views
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<UserDetailPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<ModernMapPage>();

            // 注册App
            builder.Services.AddTransient<App>();

            // 配置Json序列化器
            builder.Services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNameCaseInsensitive = true;
            });

#if DEBUG
            builder.Services.AddLogging(logging => logging.AddDebug());
#endif

            return builder.Build();
        }
    }
}