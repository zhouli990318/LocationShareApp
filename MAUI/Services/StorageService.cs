using Microsoft.Extensions.Logging;

namespace LocationShareApp.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;

        public StorageService(ILogger<StorageService> logger)
        {
            _logger = logger;
        }
        public async Task<string?> GetAsync(string key)
        {
            try
            {
                return await SecureStorage.Default.GetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取存储数据失败");
                return null;
            }
        }

        public async Task SetAsync(string key, string value)
        {
            try
            {
                await SecureStorage.Default.SetAsync(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存存储数据失败");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                SecureStorage.Default.Remove(key);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除存储数据失败");
            }
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            try
            {
                var value = await SecureStorage.Default.GetAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查存储数据失败");
                return false;
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                SecureStorage.Default.RemoveAll();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空存储数据失败");
            }
        }
    }
}