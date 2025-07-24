namespace LocationShareApp.Services
{
    public class StorageService : IStorageService
    {
        public async Task<string?> GetAsync(string key)
        {
            try
            {
                return await SecureStorage.Default.GetAsync(key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取存储数据失败: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"保存存储数据失败: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"删除存储数据失败: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"检查存储数据失败: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"清空存储数据失败: {ex.Message}");
            }
        }
    }
}