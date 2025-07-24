using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LocationShareApp.API.Services;

namespace LocationShareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;

        public LocationController(ILocationService locationService, IUserService userService)
        {
            _locationService = locationService;
            _userService = userService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            try
            {
                var userId = GetUserId();
                var location = await _locationService.AddLocationAsync(
                    userId, 
                    request.Latitude, 
                    request.Longitude, 
                    request.Address, 
                    request.Accuracy
                );

                await _userService.UpdateLastActiveAsync(userId);

                return Ok(new
                {
                    message = "位置更新成功",
                    location = new
                    {
                        id = location.Id,
                        latitude = location.Latitude,
                        longitude = location.Longitude,
                        address = location.Address,
                        timestamp = location.Timestamp,
                        accuracy = location.Accuracy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "位置更新失败", error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetLocationHistory([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                var userId = GetUserId();
                var locations = await _locationService.GetUserLocationHistoryAsync(userId, startTime, endTime);

                var result = locations.Select(l => new
                {
                    id = l.Id,
                    latitude = l.Latitude,
                    longitude = l.Longitude,
                    address = l.Address,
                    timestamp = l.Timestamp,
                    accuracy = l.Accuracy
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取位置历史失败", error = ex.Message });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserLocationHistory(int userId, [FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                var currentUserId = GetUserId();
                
                // 检查是否有权限查看该用户的位置历史
                var connectedUsers = await _userService.GetConnectedUsersAsync(currentUserId);
                if (!connectedUsers.Any(u => u.Id == userId))
                {
                    return Forbid("无权限查看该用户的位置信息");
                }

                var locations = await _locationService.GetUserLocationHistoryAsync(userId, startTime, endTime);

                var result = locations.Select(l => new
                {
                    id = l.Id,
                    latitude = l.Latitude,
                    longitude = l.Longitude,
                    address = l.Address,
                    timestamp = l.Timestamp,
                    accuracy = l.Accuracy
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取位置历史失败", error = ex.Message });
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestLocation()
        {
            try
            {
                var userId = GetUserId();
                var location = await _locationService.GetLatestLocationAsync(userId);

                if (location == null)
                    return NotFound(new { message = "未找到位置信息" });

                return Ok(new
                {
                    id = location.Id,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    address = location.Address,
                    timestamp = location.Timestamp,
                    accuracy = location.Accuracy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取最新位置失败", error = ex.Message });
            }
        }

        [HttpGet("latest/{userId}")]
        public async Task<IActionResult> GetUserLatestLocation(int userId)
        {
            try
            {
                var currentUserId = GetUserId();
                
                // 检查是否有权限查看该用户的位置信息
                var connectedUsers = await _userService.GetConnectedUsersAsync(currentUserId);
                if (!connectedUsers.Any(u => u.Id == userId))
                {
                    return Forbid("无权限查看该用户的位置信息");
                }

                var location = await _locationService.GetLatestLocationAsync(userId);

                if (location == null)
                    return NotFound(new { message = "未找到位置信息" });

                return Ok(new
                {
                    id = location.Id,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    address = location.Address,
                    timestamp = location.Timestamp,
                    accuracy = location.Accuracy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取最新位置失败", error = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }

    public class UpdateLocationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Accuracy { get; set; }
    }
}