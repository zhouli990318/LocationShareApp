using LocationShareApp.API.Models;

namespace LocationShareApp.API.Services
{
    public interface IBatteryService
    {
        Task<UserBattery> AddBatteryRecordAsync(int userId, int batteryLevel, bool isCharging);
        Task<List<UserBattery>> GetBatteryHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null);
        Task<UserBattery?> GetLatestBatteryAsync(int userId);
        Task<Dictionary<int, UserBattery?>> GetLatestBatteriesForUsersAsync(List<int> userIds);
        Task CleanupOldBatteryRecordsAsync(int daysToKeep = 30);
    }
}