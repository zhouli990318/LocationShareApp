using Microsoft.Extensions.Logging;

namespace LocationShareApp.Services
{
    public class BatteryService : IBatteryService
    {
        private bool _isMonitoring;
        private CancellationTokenSource? _cancelTokenSource;
        private readonly ILogger<BatteryService> _logger;

        public event EventHandler<BatteryEventArgs>? BatteryChanged;

        public BatteryService(ILogger<BatteryService> logger)
        {
            _logger = logger;
        }

        public async Task<double> GetBatteryLevelAsync()
        {
            try
            {
                var battery = Battery.Default;
                return await Task.FromResult(battery.ChargeLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取电量失败");
                return 0;
            }
        }

        public async Task<bool> IsBatteryChargingAsync()
        {
            try
            {
                var battery = Battery.Default;
                return await Task.FromResult(battery.State == BatteryState.Charging);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取充电状态失败");
                return false;
            }
        }

        public async Task StartBatteryMonitoringAsync()
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            _cancelTokenSource = new CancellationTokenSource();

            // 监听电量变化事件
            Battery.Default.BatteryInfoChanged += OnBatteryInfoChanged;

            // 定期检查电量状态
            _ = Task.Run(async () =>
            {
                while (_isMonitoring && !_cancelTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var batteryLevel = await GetBatteryLevelAsync();
                        var isCharging = await IsBatteryChargingAsync();
                        
                        BatteryChanged?.Invoke(this, new BatteryEventArgs(batteryLevel, isCharging));

                        await Task.Delay(60000, _cancelTokenSource.Token); // 每分钟检查一次
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "电量监控失败");
                        await Task.Delay(5000, _cancelTokenSource.Token);
                    }
                }
            }, _cancelTokenSource.Token);

            await Task.CompletedTask;
        }

        public Task StopBatteryMonitoringAsync()
        {
            _isMonitoring = false;
            _cancelTokenSource?.Cancel();
            Battery.Default.BatteryInfoChanged -= OnBatteryInfoChanged;
            return Task.CompletedTask;
        }

        private void OnBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            BatteryChanged?.Invoke(this, new BatteryEventArgs(e.ChargeLevel, e.State == BatteryState.Charging));
        }
    }
}