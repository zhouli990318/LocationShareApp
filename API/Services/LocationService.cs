using Microsoft.EntityFrameworkCore;
using LocationShareApp.API.Data;
using LocationShareApp.API.Models;

namespace LocationShareApp.API.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLocation> AddLocationAsync(int userId, double latitude, double longitude, string address, double accuracy)
        {
            var location = new UserLocation
            {
                UserId = userId,
                Latitude = latitude,
                Longitude = longitude,
                Address = address,
                Accuracy = accuracy,
                Timestamp = DateTime.UtcNow
            };

            _context.UserLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<List<UserLocation>> GetUserLocationHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null)
        {
            var query = _context.UserLocations.Where(ul => ul.UserId == userId);

            if (startTime.HasValue)
                query = query.Where(ul => ul.Timestamp >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(ul => ul.Timestamp <= endTime.Value);

            return await query.OrderByDescending(ul => ul.Timestamp).ToListAsync();
        }

        public async Task<UserLocation?> GetLatestLocationAsync(int userId)
        {
            return await _context.UserLocations
                .Where(ul => ul.UserId == userId)
                .OrderByDescending(ul => ul.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, UserLocation?>> GetLatestLocationsForUsersAsync(List<int> userIds)
        {
            var result = new Dictionary<int, UserLocation?>();

            foreach (var userId in userIds)
            {
                var location = await GetLatestLocationAsync(userId);
                result[userId] = location;
            }

            return result;
        }

        public async Task CleanupOldLocationsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldLocations = _context.UserLocations.Where(ul => ul.Timestamp < cutoffDate);
            
            _context.UserLocations.RemoveRange(oldLocations);
            await _context.SaveChangesAsync();
        }
    }
}