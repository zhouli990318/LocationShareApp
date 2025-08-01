using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LocationShareApp.API.Services;

namespace LocationShareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        private readonly IBatteryService _batteryService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILocationService locationService, IBatteryService batteryService, ILogger<UserController> logger)
        {
            _userService = userService;
            _locationService = locationService;
            _batteryService = batteryService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null)
                    return NotFound(new { message = "用户不存在" });

                return Ok(new
                {
                    id = user.Id,
                    phoneNumber = user.PhoneNumber,
                    nickName = user.NickName,
                    bindingCode = user.BindingCode,
                    avatar = user.Avatar,
                    createdAt = user.CreatedAt,
                    lastActiveAt = user.LastActiveAt,
                    isOnline = user.IsOnline
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return StatusCode(500, new { message = "获取用户信息失败", error = ex.Message });
            }
        }

        [HttpGet("connected-users")]
        public async Task<IActionResult> GetConnectedUsers()
        {
            try
            {
                var userId = GetUserId();
                var connectedUsers = await _userService.GetConnectedUsersAsync(userId);
                
                var result = new List<object>();
                foreach (var user in connectedUsers)
                {
                    var latestLocation = await _locationService.GetLatestLocationAsync(user.Id);
                    var latestBattery = await _batteryService.GetLatestBatteryAsync(user.Id);
                    
                    result.Add(new
                    {
                        id = user.Id,
                        nickName = user.NickName,
                        avatar = user.Avatar,
                        isOnline = user.IsOnline,
                        lastActiveAt = user.LastActiveAt,
                        location = latestLocation != null ? new
                        {
                            latitude = latestLocation.Latitude,
                            longitude = latestLocation.Longitude,
                            address = latestLocation.Address,
                            timestamp = latestLocation.Timestamp
                        } : null,
                        battery = latestBattery != null ? new
                        {
                            level = latestBattery.BatteryLevel,
                            isCharging = latestBattery.IsCharging,
                            timestamp = latestBattery.Timestamp
                        } : null
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取关联用户失败");
                return StatusCode(500, new { message = "获取关联用户失败", error = ex.Message });
            }
        }

        [HttpPost("add-connection")]
        public async Task<IActionResult> AddConnection([FromBody] AddConnectionRequest request)
        {
            try
            {
                var userId = GetUserId();
                var success = await _userService.AddConnectionAsync(userId, request.BindingCode, request.NickName);
                
                if (!success)
                    return BadRequest(new { message = "添加关联失败，请检查绑定码是否正确" });

                return Ok(new { message = "添加关联成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加关联失败");
                return StatusCode(500, new { message = "添加关联失败", error = ex.Message });
            }
        }

        [HttpDelete("remove-connection/{connectedUserId}")]
        public async Task<IActionResult> RemoveConnection(int connectedUserId)
        {
            try
            {
                var userId = GetUserId();
                var success = await _userService.RemoveConnectionAsync(userId, connectedUserId);
                
                if (!success)
                    return BadRequest(new { message = "移除关联失败" });

                return Ok(new { message = "移除关联成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除关联失败");
                return StatusCode(500, new { message = "移除关联失败", error = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }

    public class AddConnectionRequest
    {
        public string BindingCode { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
    }
}