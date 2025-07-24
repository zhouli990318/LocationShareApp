using Microsoft.Maui.Devices.Sensors;

namespace LocationShareApp.Services
{
    public interface ILocationService
    {
        Task<Location?> GetCurrentLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
        Task StartLocationUpdatesAsync();
        Task StopLocationUpdatesAsync();
        event EventHandler<LocationEventArgs>? LocationChanged;
    }

    public class LocationEventArgs : EventArgs
    {
        public Location Location { get; set; }
        public string Address { get; set; } = string.Empty;

        public LocationEventArgs(Location location, string address = "")
        {
            Location = location;
            Address = address;
        }
    }
}