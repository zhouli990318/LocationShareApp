using Microsoft.EntityFrameworkCore;
using LocationShareApp.API.Data;
using LocationShareApp.API.Models;

namespace LocationShareApp.API.Services
{
    public class BatteryService : IBatteryService
    {
        private readonly ApplicationDbContext _context;

        public BatteryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserBattery> AddBatteryRecordAsync(int userId, int batteryLevel, bool isCharging)
        {
            var battery = new UserBattery
            {
                UserId = userId,
                BatteryLevel = batteryLevel,
                IsCharging = isCharging,
                Timestamp = DateTime.UtcNow
            };

            _context.UserBatteries.Add(battery);
            await _context.SaveChangesAsync();
            return battery;
        }

        public async Task<List<UserBattery>> GetBatteryHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null)
        {
            var query = _context.UserBatteries.Where(ub => ub.UserId == userId);

            if (startTime.HasValue)
            {
                var utcStartTime = startTime.Value.Kind == DateTimeKind.Utc 
                    ? startTime.Value 
                    : DateTime.SpecifyKind(startTime.Value, DateTimeKind.Utc);
                query = query.Where(ul => ul.Timestamp >= utcStartTime);
            }

            if (endTime.HasValue)
            {
                var utcEndTime = endTime.Value.Kind == DateTimeKind.Utc 
                    ? endTime.Value 
                    : DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc);
                query = query.Where(ul => ul.Timestamp <= utcEndTime);
            }
            return await query.OrderByDescending(ub => ub.Timestamp).ToListAsync();
        }

        public async Task<UserBattery?> GetLatestBatteryAsync(int userId)
        {
            return await _context.UserBatteries
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, UserBattery?>> GetLatestBatteriesForUsersAsync(List<int> userIds)
        {
            var result = new Dictionary<int, UserBattery?>();

            foreach (var userId in userIds)
            {
                var battery = await GetLatestBatteryAsync(userId);
                result[userId] = battery;
            }

            return result;
        }

        public async Task CleanupOldBatteryRecordsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldRecords = _context.UserBatteries.Where(ub => ub.Timestamp < cutoffDate);
            
            _context.UserBatteries.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();
        }
    }
}