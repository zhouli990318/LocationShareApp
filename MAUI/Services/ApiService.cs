using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocationShareApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl = "http://192.168.1.201:7001/api"; // 根据实际API地址修改
        private string? _authToken;

        public ApiService(ILogger<ApiService> logger, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LocationShareApp/1.0");
            _logger = logger;
            _jsonOptions = jsonOptions.Value;
        }

        public async Task<ApiResponse<LoginResult>> LoginAsync(string phoneNumber, string password)
        {
            try
            {
                var request = new { PhoneNumber = phoneNumber, Password = password };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<LoginResult>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<LoginResult>(responseContent, _jsonOptions)
                    };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                    return new ApiResponse<LoginResult>
                    {
                        IsSuccess = false,
                        ErrorMessage = error?.GetProperty("message").GetString() ?? "登录失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录请求失败");
                return new ApiResponse<LoginResult>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex}"
                };
            }
        }

        public async Task<ApiResponse<RegisterResult>> RegisterAsync(string phoneNumber, string password, string nickName)
        {
            try
            {
                var request = new { PhoneNumber = phoneNumber, Password = password, NickName = nickName };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<RegisterResult>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<RegisterResult>(responseContent, _jsonOptions)
                    };
                }
                else
                {
                    var error = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonOptions);
                    return new ApiResponse<RegisterResult>
                    {
                        IsSuccess = false,
                        ErrorMessage = error?.GetProperty("message").GetString() ?? "注册失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册请求失败");
                return new ApiResponse<RegisterResult>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<UserProfile>> GetProfileAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_baseUrl}/user/profile");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<UserProfile>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<UserProfile>(responseContent, _jsonOptions)
                    };
                }
                else
                {
                    return new ApiResponse<UserProfile>
                    {
                        IsSuccess = false,
                        ErrorMessage = "获取用户信息失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息请求失败");
                return new ApiResponse<UserProfile>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<ConnectedUser>>> GetConnectedUsersAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_baseUrl}/user/connected-users");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<ConnectedUser>>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<List<ConnectedUser>>(responseContent, _jsonOptions) ?? new List<ConnectedUser>()
                    };
                }
                else
                {
                    return new ApiResponse<List<ConnectedUser>>
                    {
                        IsSuccess = false,
                        ErrorMessage = "获取关联用户失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取关联用户请求失败");
                return new ApiResponse<List<ConnectedUser>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> AddConnectionAsync(string bindingCode, string nickName)
        {
            try
            {
                SetAuthHeader();
                var request = new { BindingCode = bindingCode, NickName = nickName };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/user/add-connection", content);

                return new ApiResponse<bool>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    Data = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? "" : "添加关联失败"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加关联请求失败");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> RemoveConnectionAsync(int connectedUserId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/user/remove-connection/{connectedUserId}");

                return new ApiResponse<bool>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    Data = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? "" : "移除关联失败"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除关联请求失败");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateLocationAsync(double latitude, double longitude, string address, double accuracy)
        {
            try
            {
                SetAuthHeader();
                var request = new { Latitude = latitude, Longitude = longitude, Address = address, Accuracy = accuracy };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/location/update", content);

                return new ApiResponse<bool>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    Data = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? "" : "位置更新失败"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "位置更新请求失败");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateBatteryAsync(int batteryLevel, bool isCharging)
        {
            try
            {
                SetAuthHeader();
                var request = new { BatteryLevel = batteryLevel, IsCharging = isCharging };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/battery/update", content);

                return new ApiResponse<bool>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    Data = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? "" : "电量更新失败"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "电量更新请求失败");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<LocationRecord>>> GetLocationHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null)
        {
            try
            {
                SetAuthHeader();
                var url = $"{_baseUrl}/location/history/{userId}";
                if (startTime.HasValue || endTime.HasValue)
                {
                    var queryParams = new List<string>();
                    if (startTime.HasValue)
                        queryParams.Add($"startTime={startTime.Value:yyyy-MM-ddTHH:mm:ss}");
                    if (endTime.HasValue)
                        queryParams.Add($"endTime={endTime.Value:yyyy-MM-ddTHH:mm:ss}");
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<LocationRecord>>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<List<LocationRecord>>(responseContent, _jsonOptions) ?? new List<LocationRecord>()
                    };
                }
                else
                {
                    return new ApiResponse<List<LocationRecord>>
                    {
                        IsSuccess = false,
                        ErrorMessage = "获取位置历史失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取位置历史请求失败");
                return new ApiResponse<List<LocationRecord>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<BatteryRecord>>> GetBatteryHistoryAsync(int userId, DateTime? startTime = null, DateTime? endTime = null)
        {
            try
            {
                SetAuthHeader();
                var url = $"{_baseUrl}/battery/history/{userId}";
                if (startTime.HasValue || endTime.HasValue)
                {
                    var queryParams = new List<string>();
                    if (startTime.HasValue)
                        queryParams.Add($"startTime={startTime.Value:yyyy-MM-ddTHH:mm:ss}");
                    if (endTime.HasValue)
                        queryParams.Add($"endTime={endTime.Value:yyyy-MM-ddTHH:mm:ss}");
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<BatteryRecord>>
                    {
                        IsSuccess = true,
                        Data = JsonSerializer.Deserialize<List<BatteryRecord>>(responseContent, _jsonOptions) ?? new List<BatteryRecord>()
                    };
                }
                else
                {
                    return new ApiResponse<List<BatteryRecord>>
                    {
                        IsSuccess = false,
                        ErrorMessage = "获取电量历史失败"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取电量历史请求失败");
                return new ApiResponse<List<BatteryRecord>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络错误: {ex.Message}"
                };
            }
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            SetAuthHeader();
        }

        public void ClearAuthToken()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private void SetAuthHeader()
        {
            if (!string.IsNullOrEmpty(_authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
        }
    }
}