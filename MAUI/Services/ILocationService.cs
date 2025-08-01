using LocationShareApp.Models;

namespace LocationShareApp.Services
{
    public interface ILocationService
    {
        Task<Location?> GetCurrentLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
        Task StartLocationUpdatesAsync();
        Task StopLocationUpdatesAsync();
        event EventHandler<LocationEventArgs>? LocationChanged;

        // 地图标记相关功能
        event EventHandler<MapMarker>? MarkerAdded;
        event EventHandler<MapMarker>? MarkerRemoved;
        event EventHandler<MapMarker>? MarkerUpdated;

        Task<MapMarker> AddUserLocationMarkerAsync(UserLocation userLocation);
        Task UpdateUserLocationMarkerAsync(int userId, Location location);
        Task RemoveUserLocationMarkerAsync(int userId);
        Task<MapMarker> AddCustomMarkerAsync(double latitude, double longitude, string title, string description = "");
        Task RemoveMarkerAsync(string markerId);

        IEnumerable<MapMarker> GetAllMarkers();
        IEnumerable<MapMarker> GetUserLocationMarkers();

        Task<double> CalculateDistanceAsync(Location from, Location to);
        Task<MapBounds> GetOptimalMapBoundsAsync();
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