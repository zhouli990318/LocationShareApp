using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LocationShareApp.API.Services;

namespace LocationShareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BatteryController : ControllerBase
    {
        private readonly IBatteryService _batteryService;
        private readonly IUserService _userService;
        private readonly ILogger<BatteryController> _logger;

        public BatteryController(IBatteryService batteryService, IUserService userService, ILogger<BatteryController> logger)
        {
            _batteryService = batteryService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateBattery([FromBody] UpdateBatteryRequest request)
        {
            try
            {
                var userId = GetUserId();
                var battery = await _batteryService.AddBatteryRecordAsync(
                    userId, 
                    request.BatteryLevel, 
                    request.IsCharging
                );

                await _userService.UpdateLastActiveAsync(userId);

                return Ok(new
                {
                    message = "电量更新成功",
                    battery = new
                    {
                        id = battery.Id,
                        batteryLevel = battery.BatteryLevel,
                        isCharging = battery.IsCharging,
                        timestamp = battery.Timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "电量更新失败");
                return StatusCode(500, new { message = "电量更新失败", error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetBatteryHistory([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                var userId = GetUserId();
                var batteries = await _batteryService.GetBatteryHistoryAsync(userId, startTime, endTime);

                var result = batteries.Select(b => new
                {
                    id = b.Id,
                    batteryLevel = b.BatteryLevel,
                    isCharging = b.IsCharging,
                    timestamp = b.Timestamp
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取电量历史失败");
                return StatusCode(500, new { message = "获取电量历史失败", error = ex.Message });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserBatteryHistory(int userId, [FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                var currentUserId = GetUserId();
                
                // 检查是否有权限查看该用户的电量历史
                var connectedUsers = await _userService.GetConnectedUsersAsync(currentUserId);
                if (!connectedUsers.Any(u => u.Id == userId))
                {
                    return Forbid("无权限查看该用户的电量信息");
                }

                var batteries = await _batteryService.GetBatteryHistoryAsync(userId, startTime, endTime);

                var result = batteries.Select(b => new
                {
                    id = b.Id,
                    batteryLevel = b.BatteryLevel,
                    isCharging = b.IsCharging,
                    timestamp = b.Timestamp
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户电量历史失败");
                return StatusCode(500, new { message = "获取电量历史失败", error = ex.Message });
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestBattery()
        {
            try
            {
                var userId = GetUserId();
                var battery = await _batteryService.GetLatestBatteryAsync(userId);

                if (battery == null)
                    return NotFound(new { message = "未找到电量信息" });

                return Ok(new
                {
                    id = battery.Id,
                    batteryLevel = battery.BatteryLevel,
                    isCharging = battery.IsCharging,
                    timestamp = battery.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新电量失败");
                return StatusCode(500, new { message = "获取最新电量失败", error = ex.Message });
            }
        }

        [HttpGet("latest/{userId}")]
        public async Task<IActionResult> GetUserLatestBattery(int userId)
        {
            try
            {
                var currentUserId = GetUserId();
                
                // 检查是否有权限查看该用户的电量信息
                var connectedUsers = await _userService.GetConnectedUsersAsync(currentUserId);
                if (!connectedUsers.Any(u => u.Id == userId))
                {
                    return Forbid("无权限查看该用户的电量信息");
                }

                var battery = await _batteryService.GetLatestBatteryAsync(userId);

                if (battery == null)
                    return NotFound(new { message = "未找到电量信息" });

                return Ok(new
                {
                    id = battery.Id,
                    batteryLevel = battery.BatteryLevel,
                    isCharging = battery.IsCharging,
                    timestamp = battery.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户最新电量失败");
                return StatusCode(500, new { message = "获取最新电量失败", error = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }

    public class UpdateBatteryRequest
    {
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
    }
}