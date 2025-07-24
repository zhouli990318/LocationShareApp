using System.Text.Json;
using System.Reflection;

namespace LocationShareApp.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IStorageService _storageService;
        private AppConfiguration? _cachedConfiguration;
        private const string ConfigurationKey = "app_configuration";

        public ConfigurationService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default!)
        {
            try
            {
                var config = await GetAppConfigurationAsync();
                var property = GetPropertyByPath(config, key);
                
                if (property != null && property is T value)
                {
                    return value;
                }
                
                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取设置失败 {key}: {ex.Message}");
                return defaultValue;
            }
        }

        public async Task SetSettingAsync<T>(string key, T value)
        {
            try
            {
                var config = await GetAppConfigurationAsync();
                SetPropertyByPath(config, key, value);
                await SaveAppConfigurationAsync(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置配置失败 {key}: {ex.Message}");
            }
        }

        public async Task<AppConfiguration> GetAppConfigurationAsync()
        {
            if (_cachedConfiguration != null)
            {
                return _cachedConfiguration;
            }

            try
            {
                // 首先尝试从存储中加载
                var json = await _storageService.GetAsync(ConfigurationKey);
                if (!string.IsNullOrEmpty(json))
                {
                    _cachedConfiguration = JsonSerializer.Deserialize<AppConfiguration>(json);
                    if (_cachedConfiguration != null)
                    {
                        return _cachedConfiguration;
                    }
                }

                // 如果存储中没有，则从默认配置文件加载
                _cachedConfiguration = await LoadDefaultConfigurationAsync();
                return _cachedConfiguration;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载应用配置失败: {ex.Message}");
                return new AppConfiguration();
            }
        }

        public async Task SaveAppConfigurationAsync(AppConfiguration configuration)
        {
            try
            {
                var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await _storageService.SetAsync(ConfigurationKey, json);
                _cachedConfiguration = configuration;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存应用配置失败: {ex.Message}");
            }
        }

        public async Task ResetToDefaultsAsync()
        {
            try
            {
                _cachedConfiguration = await LoadDefaultConfigurationAsync();
                await SaveAppConfigurationAsync(_cachedConfiguration);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置配置失败: {ex.Message}");
            }
        }

        private async Task<AppConfiguration> LoadDefaultConfigurationAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                
                var jsonDocument = JsonDocument.Parse(json);
                var config = new AppConfiguration();

                // 手动映射配置项
                if (jsonDocument.RootElement.TryGetProperty("ApiSettings", out var apiElement))
                {
                    config.Api = JsonSerializer.Deserialize<ApiSettings>(apiElement.GetRawText()) ?? new ApiSettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("LocationSettings", out var locationElement))
                {
                    config.Location = JsonSerializer.Deserialize<LocationSettings>(locationElement.GetRawText()) ?? new LocationSettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("BatterySettings", out var batteryElement))
                {
                    config.Battery = JsonSerializer.Deserialize<BatterySettings>(batteryElement.GetRawText()) ?? new BatterySettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("SyncSettings", out var syncElement))
                {
                    config.Sync = JsonSerializer.Deserialize<SyncSettings>(syncElement.GetRawText()) ?? new SyncSettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("PerformanceSettings", out var performanceElement))
                {
                    config.Performance = JsonSerializer.Deserialize<PerformanceSettings>(performanceElement.GetRawText()) ?? new PerformanceSettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("UISettings", out var uiElement))
                {
                    config.UI = JsonSerializer.Deserialize<UISettings>(uiElement.GetRawText()) ?? new UISettings();
                }

                if (jsonDocument.RootElement.TryGetProperty("LoggingSettings", out var loggingElement))
                {
                    config.Logging = JsonSerializer.Deserialize<LoggingSettings>(loggingElement.GetRawText()) ?? new LoggingSettings();
                }

                return config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载默认配置失败: {ex.Message}");
                return new AppConfiguration();
            }
        }

        private object? GetPropertyByPath(object obj, string path)
        {
            var parts = path.Split('.');
            var current = obj;

            foreach (var part in parts)
            {
                var property = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    return null;
                }
                current = property.GetValue(current);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private void SetPropertyByPath(object obj, string path, object? value)
        {
            var parts = path.Split('.');
            var current = obj;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var property = current.GetType().GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    return;
                }
                current = property.GetValue(current);
                if (current == null)
                {
                    return;
                }
            }

            var finalProperty = current.GetType().GetProperty(parts[^1], BindingFlags.Public | BindingFlags.Instance);
            finalProperty?.SetValue(current, value);
        }
    }
}