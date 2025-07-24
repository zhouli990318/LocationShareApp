using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocationShareApp.API.Data;

namespace LocationShareApp.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var healthCheck = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Checks = new
                    {
                        Database = await CheckDatabaseAsync(),
                        Memory = CheckMemoryUsage(),
                        Disk = CheckDiskSpace()
                    }
                };

                return Ok(healthCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查失败");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // 检查数据库连接
                await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "就绪检查失败");
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow
            });
        }

        private async Task<object> CheckDatabaseAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var userCount = await _context.Users.CountAsync();
                
                return new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    UserCount = userCount,
                    ConnectionString = _context.Database.GetConnectionString()?.Split(';')[0] // 只显示服务器信息
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Error",
                    Error = ex.Message
                };
            }
        }

        private object CheckMemoryUsage()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var privateMemory = process.PrivateMemorySize64;
                
                return new
                {
                    WorkingSetMB = Math.Round(workingSet / 1024.0 / 1024.0, 2),
                    PrivateMemoryMB = Math.Round(privateMemory / 1024.0 / 1024.0, 2),
                    GCMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2)
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Error",
                    Error = ex.Message
                };
            }
        }

        private object CheckDiskSpace()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "/");
                var totalSpaceGB = Math.Round(drive.TotalSize / 1024.0 / 1024.0 / 1024.0, 2);
                var freeSpaceGB = Math.Round(drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0, 2);
                var usedSpaceGB = Math.Round(totalSpaceGB - freeSpaceGB, 2);
                
                return new
                {
                    TotalGB = totalSpaceGB,
                    UsedGB = usedSpaceGB,
                    FreeGB = freeSpaceGB,
                    UsagePercent = Math.Round((usedSpaceGB / totalSpaceGB) * 100, 1)
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Error",
                    Error = ex.Message
                };
            }
        }
    }
}