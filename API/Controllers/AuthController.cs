using Microsoft.AspNetCore.Mvc;
using LocationShareApp.API.Services;
using LocationShareApp.API.Models;

namespace LocationShareApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // 检查手机号是否已存在
                var existingUser = await _userService.GetUserByPhoneNumberAsync(request.PhoneNumber);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "手机号已被注册" });
                }

                // 创建新用户
                var user = await _userService.CreateUserAsync(request.PhoneNumber, request.Password, request.NickName);
                var token = await _userService.GenerateJwtTokenAsync(user);

                return Ok(new
                {
                    message = "注册成功",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        phoneNumber = user.PhoneNumber,
                        nickName = user.NickName,
                        bindingCode = user.BindingCode,
                        avatar = user.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册失败");
                return StatusCode(500, new { message = "注册失败", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userService.GetUserByPhoneNumberAsync(request.PhoneNumber);
                if (user == null)
                {
                    return BadRequest(new { message = "用户不存在" });
                }

                var isValidPassword = await _userService.ValidatePasswordAsync(user, request.Password);
                if (!isValidPassword)
                {
                    return BadRequest(new { message = "密码错误" });
                }

                var token = await _userService.GenerateJwtTokenAsync(user);
                await _userService.UpdateLastActiveAsync(user.Id);

                return Ok(new
                {
                    message = "登录成功",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        phoneNumber = user.PhoneNumber,
                        nickName = user.NickName,
                        bindingCode = user.BindingCode,
                        avatar = user.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return StatusCode(500, new { message = "登录失败", error = ex.Message });
            }
        }
    }

    public class RegisterRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}