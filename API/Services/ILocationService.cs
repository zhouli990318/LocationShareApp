using LocationShareApp.API.Models;

namespace LocationShareApp.API.Services
{
    public interface ILocationService
    {
        Task<UserLocation> AddLocationAsync(int userId, double latitude, double longitude, string address, double accuracy);
        Task<List<UserLocation>> GetUserLocationHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null);
        Task<UserLocation?> GetLatestLocationAsync(int userId);
        Task<Dictionary<int, UserLocation?>> GetLatestLocationsForUsersAsync(List<int> userIds);
        Task CleanupOldLocationsAsync(int daysToKeep = 30);
    }
}