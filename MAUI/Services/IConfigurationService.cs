namespace LocationShareApp.Services
{
    public interface IConfigurationService
    {
        Task<T> GetSettingAsync<T>(string key, T defaultValue = default!);
        Task SetSettingAsync<T>(string key, T value);
        Task<AppConfiguration> GetAppConfigurationAsync();
        Task SaveAppConfigurationAsync(AppConfiguration configuration);
        Task ResetToDefaultsAsync();
    }

    public class AppConfiguration
    {
        public ApiSettings Api { get; set; } = new();
        public LocationSettings Location { get; set; } = new();
        public BatterySettings Battery { get; set; } = new();
        public SyncSettings Sync { get; set; } = new();
        public PerformanceSettings Performance { get; set; } = new();
        public UISettings UI { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "https://localhost:7001/api";
        public string SignalRHubUrl { get; set; } = "https://localhost:7001/locationHub";
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryAttempts { get; set; } = 3;
    }

    public class LocationSettings
    {
        public int UpdateIntervalSeconds { get; set; } = 30;
        public double AccuracyThresholdMeters { get; set; } = 50;
        public bool EnableBackgroundUpdates { get; set; } = true;
        public int MaxLocationHistoryDays { get; set; } = 30;
    }

    public class BatterySettings
    {
        public int UpdateIntervalSeconds { get; set; } = 60;
        public double LowBatteryThreshold { get; set; } = 0.15;
        public double CriticalBatteryThreshold { get; set; } = 0.05;
        public int MaxBatteryHistoryDays { get; set; } = 30;
    }

    public class SyncSettings
    {
        public int SyncIntervalMinutes { get; set; } = 5;
        public int MaxCacheItems { get; set; } = 1000;
        public bool EnableOfflineMode { get; set; } = true;
        public bool AutoSyncOnNetworkRestore { get; set; } = true;
    }

    public class PerformanceSettings
    {
        public bool EnablePerformanceMonitoring { get; set; } = true;
        public int MonitoringIntervalSeconds { get; set; } = 30;
        public int MaxPerformanceLogs { get; set; } = 1000;
        public int MemoryWarningThresholdMB { get; set; } = 200;
        public int MemoryCriticalThresholdMB { get; set; } = 300;
    }

    public class UISettings
    {
        public string Theme { get; set; } = "Light";
        public string PrimaryColor { get; set; } = "#1976D2";
        public string SecondaryColor { get; set; } = "#E3F2FD";
        public bool EnableAnimations { get; set; } = true;
        public string MapType { get; set; } = "Street";
    }

    public class LoggingSettings
    {
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Information";
        public int MaxLogFiles { get; set; } = 10;
        public int MaxLogSizeMB { get; set; } = 5;
    }
}