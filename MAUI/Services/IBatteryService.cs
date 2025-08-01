namespace LocationShareApp.Services
{
    public interface IBatteryService
    {
        Task<double> GetBatteryLevelAsync();
        Task<bool> IsBatteryChargingAsync();
        Task StartBatteryMonitoringAsync();
        Task StopBatteryMonitoringAsync();
        event EventHandler<BatteryEventArgs>? BatteryChanged;
    }

    public class BatteryEventArgs : EventArgs
    {
        public double BatteryLevel { get; set; }
        public bool IsCharging { get; set; }

        public BatteryEventArgs(double batteryLevel, bool isCharging)
        {
            BatteryLevel = batteryLevel;
            IsCharging = isCharging;
        }
    }
}