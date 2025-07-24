using System.Text.Json;

namespace LocationShareApp.Services
{
    public class DataCacheService : IDataCacheService
    {
        private readonly IStorageService _storageService;
        private readonly IApiService _apiService;
        private readonly ISignalRService _signalRService;
        
        private const string LocationCacheKey = "cached_locations";
        private const string BatteryCacheKey = "cached_batteries";

        public DataCacheService(IStorageService storageService, IApiService apiService, ISignalRService signalRService)
        {
            _storageService = storageService;
            _apiService = apiService;
            _signalRService = signalRService;
        }

        public async Task CacheLocationDataAsync(LocationCacheItem item)
        {
            try
            {
                var cachedItems = await GetCachedLocationDataAsync();
                cachedItems.Add(item);
                
                // 限制缓存数量，保留最近100条记录
                if (cachedItems.Count > 100)
                {
                    cachedItems = cachedItems.OrderByDescending(x => x.Timestamp).Take(100).ToList();
                }
                
                var json = JsonSerializer.Serialize(cachedItems);
                await _storageService.SetAsync(LocationCacheKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"缓存位置数据失败: {ex.Message}");
            }
        }

        public async Task CacheBatteryDataAsync(BatteryCacheItem item)
        {
            try
            {
                var cachedItems = await GetCachedBatteryDataAsync();
                cachedItems.Add(item);
                
                // 限制缓存数量，保留最近200条记录
                if (cachedItems.Count > 200)
                {
                    cachedItems = cachedItems.OrderByDescending(x => x.Timestamp).Take(200).ToList();
                }
                
                var json = JsonSerializer.Serialize(cachedItems);
                await _storageService.SetAsync(BatteryCacheKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"缓存电量数据失败: {ex.Message}");
            }
        }

        public async Task<List<LocationCacheItem>> GetCachedLocationDataAsync()
        {
            try
            {
                var json = await _storageService.GetAsync(LocationCacheKey);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<List<LocationCacheItem>>(json) ?? new List<LocationCacheItem>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取缓存位置数据失败: {ex.Message}");
            }
            
            return new List<LocationCacheItem>();
        }

        public async Task<List<BatteryCacheItem>> GetCachedBatteryDataAsync()
        {
            try
            {
                var json = await _storageService.GetAsync(BatteryCacheKey);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<List<BatteryCacheItem>>(json) ?? new List<BatteryCacheItem>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取缓存电量数据失败: {ex.Message}");
            }
            
            return new List<BatteryCacheItem>();
        }

        public async Task SyncCachedDataAsync()
        {
            try
            {
                await SyncCachedLocationDataAsync();
                await SyncCachedBatteryDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"同步缓存数据失败: {ex.Message}");
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                await _storageService.RemoveAsync(LocationCacheKey);
                await _storageService.RemoveAsync(BatteryCacheKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空缓存失败: {ex.Message}");
            }
        }

        public async Task<int> GetCacheCountAsync()
        {
            try
            {
                var locationCount = (await GetCachedLocationDataAsync()).Count(x => !x.IsSynced);
                var batteryCount = (await GetCachedBatteryDataAsync()).Count(x => !x.IsSynced);
                return locationCount + batteryCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取缓存数量失败: {ex.Message}");
                return 0;
            }
        }

        private async Task SyncCachedLocationDataAsync()
        {
            var cachedItems = await GetCachedLocationDataAsync();
            var unsyncedItems = cachedItems.Where(x => !x.IsSynced).OrderBy(x => x.Timestamp).ToList();
            
            foreach (var item in unsyncedItems)
            {
                try
                {
                    var result = await _apiService.UpdateLocationAsync(
                        item.Latitude, 
                        item.Longitude, 
                        item.Address, 
                        item.Accuracy);
                    
                    if (result.IsSuccess)
                    {
                        item.IsSynced = true;
                        
                        // 通过SignalR发送实时更新
                        if (_signalRService.IsConnected)
                        {
                            await _signalRService.SendLocationUpdateAsync(
                                item.Latitude, 
                                item.Longitude, 
                                item.Address, 
                                item.Accuracy);
                        }
                    }
                    else
                    {
                        // 同步失败，跳出循环避免重复失败
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"同步位置数据失败: {ex.Message}");
                    break;
                }
                
                // 避免请求过于频繁
                await Task.Delay(500);
            }
            
            // 更新缓存
            if (unsyncedItems.Any(x => x.IsSynced))
            {
                var json = JsonSerializer.Serialize(cachedItems);
                await _storageService.SetAsync(LocationCacheKey, json);
            }
        }

        private async Task SyncCachedBatteryDataAsync()
        {
            var cachedItems = await GetCachedBatteryDataAsync();
            var unsyncedItems = cachedItems.Where(x => !x.IsSynced).OrderBy(x => x.Timestamp).ToList();
            
            foreach (var item in unsyncedItems)
            {
                try
                {
                    var result = await _apiService.UpdateBatteryAsync(item.BatteryLevel, item.IsCharging);
                    
                    if (result.IsSuccess)
                    {
                        item.IsSynced = true;
                        
                        // 通过SignalR发送实时更新
                        if (_signalRService.IsConnected)
                        {
                            await _signalRService.SendBatteryUpdateAsync(item.BatteryLevel, item.IsCharging);
                        }
                    }
                    else
                    {
                        // 同步失败，跳出循环避免重复失败
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"同步电量数据失败: {ex.Message}");
                    break;
                }
                
                // 避免请求过于频繁
                await Task.Delay(500);
            }
            
            // 更新缓存
            if (unsyncedItems.Any(x => x.IsSynced))
            {
                var json = JsonSerializer.Serialize(cachedItems);
                await _storageService.SetAsync(BatteryCacheKey, json);
            }
        }
    }
}