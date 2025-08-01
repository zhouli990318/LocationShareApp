using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationShareApp.Models;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace LocationShareApp.Services
{
    public interface IMauiMapService
    {
        Task InitializeAsync();
        Task SetCenterAsync(double latitude, double longitude);
        Task SetZoomLevelAsync(double zoomLevel);
        Task AddPinAsync(MapPin pin);
        Task RemovePinAsync(MapPin pin);
        Task ClearPinsAsync();
        Task<MapSpan> GetVisibleRegionAsync();
        Task MoveToRegionAsync(MapSpan mapSpan);
        event EventHandler<MapClickedEventArgs>? MapClicked;
    }

    public class MauiMapService : IMauiMapService
    {
        private Map? _map;
        private readonly List<Pin> _pins;

        public event EventHandler<MapClickedEventArgs>? MapClicked;

        public MauiMapService()
        {
            _pins = new List<Pin>();
        }

        public Task InitializeAsync()
        {
            // MAUI Maps 不需要特殊的初始化
            return Task.CompletedTask;
        }

        public void SetMap(Map map)
        {
            _map = map;
            if (_map != null)
            {
                _map.MapClicked += OnMapClicked;
            }
        }

        public Task SetCenterAsync(double latitude, double longitude)
        {
            if (_map != null)
            {
                var location = new Microsoft.Maui.Devices.Sensors.Location(latitude, longitude);
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1));
                _map.MoveToRegion(mapSpan);
            }
            return Task.CompletedTask;
        }

        public Task SetZoomLevelAsync(double zoomLevel)
        {
            // MAUI Maps 使用 Distance 来控制缩放级别
            if (_map != null && _map.VisibleRegion != null)
            {
                var distance = Distance.FromKilometers(Math.Pow(2, 15 - zoomLevel));
                var mapSpan = MapSpan.FromCenterAndRadius(_map.VisibleRegion.Center, distance);
                _map.MoveToRegion(mapSpan);
            }
            return Task.CompletedTask;
        }

        public Task AddPinAsync(MapPin mapPin)
        {
            if (_map != null)
            {
                var pin = new Pin
                {
                    Label = mapPin.Title,
                    Address = mapPin.Description,
                    Type = PinType.Place,
                    Location = new Microsoft.Maui.Devices.Sensors.Location(mapPin.Latitude, mapPin.Longitude)
                };

                _pins.Add(pin);
                _map.Pins.Add(pin);
            }
            return Task.CompletedTask;
        }

        public Task RemovePinAsync(MapPin mapPin)
        {
            if (_map != null)
            {
                var pin = _pins.FirstOrDefault(p => 
                    Math.Abs(p.Location.Latitude - mapPin.Latitude) < 0.0001 && 
                    Math.Abs(p.Location.Longitude - mapPin.Longitude) < 0.0001);
                
                if (pin != null)
                {
                    _pins.Remove(pin);
                    _map.Pins.Remove(pin);
                }
            }
            return Task.CompletedTask;
        }

        public Task ClearPinsAsync()
        {
            if (_map != null)
            {
                _pins.Clear();
                _map.Pins.Clear();
            }
            return Task.CompletedTask;
        }

        public Task<MapSpan> GetVisibleRegionAsync()
        {
            return Task.FromResult(_map?.VisibleRegion ?? MapSpan.FromCenterAndRadius(
                new Microsoft.Maui.Devices.Sensors.Location(0, 0), 
                Distance.FromKilometers(1)));
        }

        public Task MoveToRegionAsync(MapSpan mapSpan)
        {
            _map?.MoveToRegion(mapSpan);
            return Task.CompletedTask;
        }

        private void OnMapClicked(object? sender, MapClickedEventArgs e)
        {
            MapClicked?.Invoke(sender, e);
        }
    }

    public class MapPin
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}