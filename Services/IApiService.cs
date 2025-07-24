using LocationShareApp.Models;

namespace LocationShareApp.Services
{
    public interface IApiService
    {
        Task<ApiResponse<LoginResult>> LoginAsync(string phoneNumber, string password);
        Task<ApiResponse<RegisterResult>> RegisterAsync(string phoneNumber, string password, string nickName);
        Task<ApiResponse<UserProfile>> GetProfileAsync();
        Task<ApiResponse<List<ConnectedUser>>> GetConnectedUsersAsync();
        Task<ApiResponse<bool>> AddConnectionAsync(string bindingCode, string nickName);
        Task<ApiResponse<bool>> RemoveConnectionAsync(int connectedUserId);
        Task<ApiResponse<bool>> UpdateLocationAsync(double latitude, double longitude, string address, double accuracy);
        Task<ApiResponse<bool>> UpdateBatteryAsync(int batteryLevel, bool isCharging);
        Task<ApiResponse<List<LocationRecord>>> GetLocationHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null);
        Task<ApiResponse<List<BatteryRecord>>> GetBatteryHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null);
        void SetAuthToken(string token);
        void ClearAuthToken();
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class RegisterResult
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string BindingCode { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }

    public class UserProfile
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string BindingCode { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ConnectedUser
    {
        public int Id { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastActiveAt { get; set; }
        public LocationInfo? Location { get; set; }
        public BatteryInfo? Battery { get; set; }
    }

    public class LocationInfo
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class BatteryInfo
    {
        public int Level { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class LocationRecord
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Accuracy { get; set; }
    }

    public class BatteryRecord
    {
        public int Id { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
    }
}