using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置中间件
app.UseCors();
app.UseRouting();

// 健康检查端点
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Service = "位置共享API服务",
    Version = "1.0.0"
});

// API路由
app.MapGet("/api/status", () => new
{
    Message = "位置共享API服务运行正常",
    Features = new[]
    {
        "用户注册登录",
        "位置数据同步",
        "电量监控",
        "实时通信",
        "设备信息管理"
    },
    Timestamp = DateTime.UtcNow
});

// 用户相关API
app.MapPost("/api/auth/register", (UserRegisterRequest request) => new
{
    Success = true,
    Message = "用户注册成功",
    BindingCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
    UserId = Guid.NewGuid()
});

app.MapPost("/api/auth/login", (UserLoginRequest request) => new
{
    Success = true,
    Message = "登录成功",
    Token = "demo_jwt_token_" + DateTime.UtcNow.Ticks,
    User = new
    {
        Id = Guid.NewGuid(),
        PhoneNumber = request.PhoneNumber,
        Nickname = "演示用户",
        BindingCode = "DEMO1234"
    }
});

// 位置相关API
app.MapGet("/api/location/users", () => new[]
{
    new
    {
        Id = Guid.NewGuid(),
        Nickname = "张三",
        Latitude = 39.9042,
        Longitude = 116.4074,
        Address = "北京市朝阳区",
        LastUpdate = DateTime.UtcNow.AddMinutes(-5),
        BatteryLevel = 85,
        IsCharging = false
    },
    new
    {
        Id = Guid.NewGuid(),
        Nickname = "李四",
        Latitude = 31.2304,
        Longitude = 121.4737,
        Address = "上海市黄浦区",
        LastUpdate = DateTime.UtcNow.AddMinutes(-2),
        BatteryLevel = 92,
        IsCharging = true
    }
});

app.MapPost("/api/location/update", (LocationUpdateRequest request) => new
{
    Success = true,
    Message = "位置更新成功",
    Timestamp = DateTime.UtcNow
});

// 电量相关API
app.MapGet("/api/battery/history/{userId}", (string userId) => new
{
    UserId = userId,
    History = Enumerable.Range(0, 24).Select(i => new
    {
        Timestamp = DateTime.UtcNow.AddHours(-i),
        BatteryLevel = Math.Max(20, 100 - i * 2 + Random.Shared.Next(-10, 10)),
        IsCharging = Random.Shared.Next(0, 10) > 7
    }).Reverse()
});

// SignalR Hub
app.MapHub<LocationHub>("/locationHub");

app.Run();

// 数据模型
public record UserRegisterRequest(string PhoneNumber, string Password, string Nickname);
public record UserLoginRequest(string PhoneNumber, string Password);
public record LocationUpdateRequest(double Latitude, double Longitude, string Address);

// SignalR Hub
public class LocationHub : Hub
{
    public async Task JoinGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Group($"user_{userId}").SendAsync("UserConnected", userId);
    }

    public async Task SendLocationUpdate(string userId, double latitude, double longitude, string address)
    {
        await Clients.All.SendAsync("LocationUpdated", new
        {
            UserId = userId,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendBatteryUpdate(string userId, int batteryLevel, bool isCharging)
    {
        await Clients.All.SendAsync("BatteryUpdated", new
        {
            UserId = userId,
            BatteryLevel = batteryLevel,
            IsCharging = isCharging,
            Timestamp = DateTime.UtcNow
        });
    }
}