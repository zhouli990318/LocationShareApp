using System.Text.Json;

namespace LocationShareApp.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly IStorageService _storageService;
        private readonly List<ErrorLog> _errorLogs;
        private const string ErrorLogsKey = "error_logs";
        private const int MaxLogCount = 500;

        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        public ErrorHandlingService(IStorageService storageService)
        {
            _storageService = storageService;
            _errorLogs = new List<ErrorLog>();
            
            // 加载已保存的错误日志
            _ = LoadErrorLogsAsync();
        }

        public async Task LogErrorAsync(Exception exception, string context = "")
        {
            var errorLog = new ErrorLog
            {
                Level = "Error",
                Message = exception.Message,
                Context = context,
                StackTrace = exception.StackTrace ?? string.Empty,
                DeviceInfo = GetDeviceInfo(),
                AppVersion = AppInfo.Current.VersionString
            };

            await AddErrorLogAsync(errorLog);
            OnErrorOccurred(errorLog);
        }

        public async Task LogWarningAsync(string message, string context = "")
        {
            var errorLog = new ErrorLog
            {
                Level = "Warning",
                Message = message,
                Context = context,
                DeviceInfo = GetDeviceInfo(),
                AppVersion = AppInfo.Current.VersionString
            };

            await AddErrorLogAsync(errorLog);
        }

        public async Task LogInfoAsync(string message, string context = "")
        {
            var errorLog = new ErrorLog
            {
                Level = "Info",
                Message = message,
                Context = context,
                DeviceInfo = GetDeviceInfo(),
                AppVersion = AppInfo.Current.VersionString
            };

            await AddErrorLogAsync(errorLog);
        }

        public async Task<List<ErrorLog>> GetErrorLogsAsync()
        {
            return await Task.FromResult(_errorLogs.ToList());
        }

        public async Task ClearErrorLogsAsync()
        {
            _errorLogs.Clear();
            await SaveErrorLogsAsync();
        }

        public async Task HandleUnhandledExceptionAsync(Exception exception)
        {
            try
            {
                await LogErrorAsync(exception, "UnhandledException");
                
                // 显示用户友好的错误消息
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "应用错误", 
                        "应用遇到了一个意外错误，我们已记录此问题。", 
                        "确定");
                }
            }
            catch (Exception logException)
            {
                System.Diagnostics.Debug.WriteLine($"记录未处理异常失败: {logException.Message}");
            }
        }

        private async Task AddErrorLogAsync(ErrorLog errorLog)
        {
            try
            {
                _errorLogs.Add(errorLog);
                
                // 限制日志数量
                if (_errorLogs.Count > MaxLogCount)
                {
                    _errorLogs.RemoveRange(0, 100);
                }

                await SaveErrorLogsAsync();
                
                // 输出到调试控制台
                System.Diagnostics.Debug.WriteLine($"[{errorLog.Level}] {errorLog.Context}: {errorLog.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加错误日志失败: {ex.Message}");
            }
        }

        private async Task LoadErrorLogsAsync()
        {
            try
            {
                var json = await _storageService.GetAsync(ErrorLogsKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var logs = JsonSerializer.Deserialize<List<ErrorLog>>(json);
                    if (logs != null)
                    {
                        _errorLogs.AddRange(logs);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载错误日志失败: {ex.Message}");
            }
        }

        private async Task SaveErrorLogsAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_errorLogs);
                await _storageService.SetAsync(ErrorLogsKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存错误日志失败: {ex.Message}");
            }
        }

        private string GetDeviceInfo()
        {
            try
            {
                return $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString} - {DeviceInfo.Current.Model}";
            }
            catch
            {
                return "Unknown Device";
            }
        }

        private void OnErrorOccurred(ErrorLog errorLog)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs { ErrorLog = errorLog });
        }
    }
}