using Microsoft.AspNetCore.SignalR.Client;

namespace LocationShareApp.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl = "https://localhost:7001/locationHub"; // 根据实际地址修改

        public event EventHandler<UserLocationUpdatedEventArgs>? UserLocationUpdated;
        public event EventHandler<UserBatteryUpdatedEventArgs>? UserBatteryUpdated;
        public event EventHandler<UserOnlineEventArgs>? UserOnline;
        public event EventHandler<UserOfflineEventArgs>? UserOffline;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task StartConnectionAsync(string token)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // 注册事件处理器
                _hubConnection.On<dynamic>("LocationUpdated", (data) =>
                {
                    UserLocationUpdated?.Invoke(this, new UserLocationUpdatedEventArgs
                    {
                        UserId = data.UserId,
                        Latitude = data.Latitude,
                        Longitude = data.Longitude,
                        Address = data.Address,
                        Timestamp = data.Timestamp
                    });
                });

                _hubConnection.On<dynamic>("BatteryUpdated", (data) =>
                {
                    UserBatteryUpdated?.Invoke(this, new UserBatteryUpdatedEventArgs
                    {
                        UserId = data.UserId,
                        BatteryLevel = data.BatteryLevel,
                        IsCharging = data.IsCharging,
                        Timestamp = data.Timestamp
                    });
                });

                _hubConnection.On<int>("UserOnline", (userId) =>
                {
                    UserOnline?.Invoke(this, new UserOnlineEventArgs { UserId = userId });
                });

                _hubConnection.On<int>("UserOffline", (userId) =>
                {
                    UserOffline?.Invoke(this, new UserOfflineEventArgs { UserId = userId });
                });

                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR连接失败: {ex.Message}");
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public async Task SendLocationUpdateAsync(double latitude, double longitude, string address, double accuracy)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("UpdateLocation", latitude, longitude, address, accuracy);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"发送位置更新失败: {ex.Message}");
                }
            }
        }

        public async Task SendBatteryUpdateAsync(int batteryLevel, bool isCharging)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("UpdateBattery", batteryLevel, isCharging);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"发送电量更新失败: {ex.Message}");
                }
            }
        }
    }
}