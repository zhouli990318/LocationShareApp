namespace LocationShareApp.Services
{
    public interface IDataCacheService
    {
        Task CacheLocationDataAsync(LocationCacheItem item);
        Task CacheBatteryDataAsync(BatteryCacheItem item);
        Task<List<LocationCacheItem>> GetCachedLocationDataAsync();
        Task<List<BatteryCacheItem>> GetCachedBatteryDataAsync();
        Task SyncCachedDataAsync();
        Task ClearCacheAsync();
        Task<int> GetCacheCountAsync();
    }

    public class LocationCacheItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Accuracy { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSynced { get; set; }
    }

    public class BatteryCacheItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSynced { get; set; }
    }
}