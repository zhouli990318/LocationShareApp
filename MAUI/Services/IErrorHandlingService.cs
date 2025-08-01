namespace LocationShareApp.Services
{
    public interface IErrorHandlingService
    {
        Task LogErrorAsync(Exception exception, string context = "");
        Task LogWarningAsync(string message, string context = "");
        Task LogInfoAsync(string message, string context = "");
        Task<List<ErrorLog>> GetErrorLogsAsync();
        Task ClearErrorLogsAsync();
        Task HandleUnhandledExceptionAsync(Exception exception);
        event EventHandler<ErrorEventArgs>? ErrorOccurred;
    }

    public class ErrorLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string DeviceInfo { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class ErrorEventArgs : EventArgs
    {
        public ErrorLog ErrorLog { get; set; } = new();
        public bool IsHandled { get; set; }
    }
}