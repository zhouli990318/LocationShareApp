using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LocationShareApp.API.Services
{
    [Authorize]
    public class LocationHub : Hub
    {
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        private readonly IBatteryService _batteryService;

        public LocationHub(IUserService userService, ILocationService locationService, IBatteryService batteryService)
        {
            _userService = userService;
            _locationService = locationService;
            _batteryService = batteryService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                await _userService.UpdateLastActiveAsync(userId.Value);
                
                // 通知关联用户该用户已上线
                var connectedUsers = await _userService.GetConnectedUsersAsync(userId.Value);
                foreach (var user in connectedUsers)
                {
                    await Clients.Group($"User_{user.Id}").SendAsync("UserOnline", userId.Value);
                }
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                // 通知关联用户该用户已离线
                var connectedUsers = await _userService.GetConnectedUsersAsync(userId.Value);
                foreach (var user in connectedUsers)
                {
                    await Clients.Group($"User_{user.Id}").SendAsync("UserOffline", userId.Value);
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateLocation(double latitude, double longitude, string address, double accuracy)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var location = await _locationService.AddLocationAsync(userId.Value, latitude, longitude, address, accuracy);
            
            // 通知关联用户位置更新
            var connectedUsers = await _userService.GetConnectedUsersAsync(userId.Value);
            foreach (var user in connectedUsers)
            {
                await Clients.Group($"User_{user.Id}").SendAsync("LocationUpdated", new
                {
                    UserId = userId.Value,
                    Latitude = latitude,
                    Longitude = longitude,
                    Address = address,
                    Timestamp = location.Timestamp
                });
            }
        }

        public async Task UpdateBattery(int batteryLevel, bool isCharging)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var battery = await _batteryService.AddBatteryRecordAsync(userId.Value, batteryLevel, isCharging);
            
            // 通知关联用户电量更新
            var connectedUsers = await _userService.GetConnectedUsersAsync(userId.Value);
            foreach (var user in connectedUsers)
            {
                await Clients.Group($"User_{user.Id}").SendAsync("BatteryUpdated", new
                {
                    UserId = userId.Value,
                    BatteryLevel = batteryLevel,
                    IsCharging = isCharging,
                    Timestamp = battery.Timestamp
                });
            }
        }

        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}