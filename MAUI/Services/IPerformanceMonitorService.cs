namespace LocationShareApp.Services
{
    public interface IPerformanceMonitorService
    {
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        Task<PerformanceMetrics> GetCurrentMetricsAsync();
        Task<List<PerformanceLog>> GetPerformanceLogsAsync();
        Task ClearPerformanceLogsAsync();
        event EventHandler<PerformanceEventArgs>? PerformanceAlert;
    }

    public class PerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public long AvailableMemory { get; set; }
        public double BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public string NetworkType { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class PerformanceLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Event { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public PerformanceLevel Level { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class PerformanceEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public PerformanceLevel Level { get; set; }
        public PerformanceMetrics? Metrics { get; set; }
    }

    public enum PerformanceLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
}