namespace LocationShareApp.Services
{
    public class PerformanceMonitorService : IPerformanceMonitorService
    {
        private readonly IBatteryService _batteryService;
        private readonly IDeviceInfoService _deviceInfoService;
        private readonly List<PerformanceLog> _performanceLogs;
        private Timer? _monitoringTimer;
        private bool _isMonitoring;

        public event EventHandler<PerformanceEventArgs>? PerformanceAlert;

        public PerformanceMonitorService(IBatteryService batteryService, IDeviceInfoService deviceInfoService)
        {
            _batteryService = batteryService;
            _deviceInfoService = deviceInfoService;
            _performanceLogs = new List<PerformanceLog>();
        }

        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            try
            {
                _isMonitoring = true;
                
                // 每30秒监控一次性能指标
                _monitoringTimer = new Timer(async _ => await MonitorPerformanceAsync(), 
                    null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                
                LogPerformance("性能监控已启动", PerformanceLevel.Info);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogPerformance($"启动性能监控失败: {ex.Message}", PerformanceLevel.Error);
            }
        }

        public async Task StopMonitoringAsync()
        {
            if (!_isMonitoring) return;

            try
            {
                _isMonitoring = false;
                _monitoringTimer?.Dispose();
                _monitoringTimer = null;
                
                LogPerformance("性能监控已停止", PerformanceLevel.Info);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogPerformance($"停止性能监控失败: {ex.Message}", PerformanceLevel.Error);
            }
        }

        public async Task<PerformanceMetrics> GetCurrentMetricsAsync()
        {
            try
            {
                var batteryLevel = await _batteryService.GetBatteryLevelAsync();
                var isCharging = await _batteryService.IsBatteryChargingAsync();
                var networkInfo = await _deviceInfoService.GetNetworkInfoAsync();

                return new PerformanceMetrics
                {
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    AvailableMemory = GetAvailableMemory(),
                    BatteryLevel = batteryLevel,
                    IsCharging = isCharging,
                    NetworkType = networkInfo.NetworkType,
                    IsConnected = networkInfo.IsConnected,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                LogPerformance($"获取性能指标失败: {ex.Message}", PerformanceLevel.Error);
                return new PerformanceMetrics();
            }
        }

        public async Task<List<PerformanceLog>> GetPerformanceLogsAsync()
        {
            return await Task.FromResult(_performanceLogs.ToList());
        }

        public async Task ClearPerformanceLogsAsync()
        {
            _performanceLogs.Clear();
            await Task.CompletedTask;
        }

        private async Task MonitorPerformanceAsync()
        {
            if (!_isMonitoring) return;

            try
            {
                var metrics = await GetCurrentMetricsAsync();
                
                // 检查内存使用情况
                CheckMemoryUsage(metrics);
                
                // 检查电池电量
                CheckBatteryLevel(metrics);
                
                // 检查网络连接
                CheckNetworkConnection(metrics);
                
                // 记录性能指标
                LogPerformance($"性能监控 - 内存: {metrics.MemoryUsage / 1024 / 1024}MB, 电量: {metrics.BatteryLevel:P0}, 网络: {metrics.NetworkType}", 
                    PerformanceLevel.Info);
            }
            catch (Exception ex)
            {
                LogPerformance($"性能监控异常: {ex.Message}", PerformanceLevel.Error);
            }
        }

        private void CheckMemoryUsage(PerformanceMetrics metrics)
        {
            var memoryUsageMB = metrics.MemoryUsage / 1024 / 1024;
            
            if (memoryUsageMB > 200) // 超过200MB
            {
                var message = $"内存使用过高: {memoryUsageMB}MB";
                LogPerformance(message, PerformanceLevel.Warning);
                OnPerformanceAlert(message, PerformanceLevel.Warning, metrics);
            }
            else if (memoryUsageMB > 300) // 超过300MB
            {
                var message = $"内存使用严重过高: {memoryUsageMB}MB";
                LogPerformance(message, PerformanceLevel.Critical);
                OnPerformanceAlert(message, PerformanceLevel.Critical, metrics);
            }
        }

        private void CheckBatteryLevel(PerformanceMetrics metrics)
        {
            if (metrics.BatteryLevel < 0.15 && !metrics.IsCharging) // 电量低于15%且未充电
            {
                var message = $"电量过低: {metrics.BatteryLevel:P0}";
                LogPerformance(message, PerformanceLevel.Warning);
                OnPerformanceAlert(message, PerformanceLevel.Warning, metrics);
            }
            else if (metrics.BatteryLevel < 0.05) // 电量低于5%
            {
                var message = $"电量严重不足: {metrics.BatteryLevel:P0}";
                LogPerformance(message, PerformanceLevel.Critical);
                OnPerformanceAlert(message, PerformanceLevel.Critical, metrics);
            }
        }

        private void CheckNetworkConnection(PerformanceMetrics metrics)
        {
            if (!metrics.IsConnected)
            {
                var message = "网络连接断开";
                LogPerformance(message, PerformanceLevel.Warning);
                OnPerformanceAlert(message, PerformanceLevel.Warning, metrics);
            }
        }

        private double GetCpuUsage()
        {
            // 简化的CPU使用率获取，实际实现需要平台特定代码
            return 0.0;
        }

        private long GetMemoryUsage()
        {
            // 简化的内存使用获取，实际实现需要平台特定代码
            try
            {
                return GC.GetTotalMemory(false);
            }
            catch
            {
                return 0;
            }
        }

        private long GetAvailableMemory()
        {
            // 简化的可用内存获取，实际实现需要平台特定代码
            return 0;
        }

        private void LogPerformance(string message, PerformanceLevel level)
        {
            var log = new PerformanceLog
            {
                Event = "性能监控",
                Details = message,
                Level = level,
                Timestamp = DateTime.Now
            };

            _performanceLogs.Add(log);
            
            // 限制日志数量
            if (_performanceLogs.Count > 1000)
            {
                _performanceLogs.RemoveRange(0, 100);
            }

            System.Diagnostics.Debug.WriteLine($"[Performance] {level}: {message}");
        }

        private void OnPerformanceAlert(string message, PerformanceLevel level, PerformanceMetrics metrics)
        {
            PerformanceAlert?.Invoke(this, new PerformanceEventArgs
            {
                Message = message,
                Level = level,
                Metrics = metrics
            });
        }
    }
}