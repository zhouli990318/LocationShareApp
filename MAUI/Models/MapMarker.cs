namespace LocationShareApp.Models
{
    public class MapMarker
    {
        public string Id { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public MapMarkerType MarkerType { get; set; }
        public bool IsVisible { get; set; } = true;
        public string IconPath { get; set; } = string.Empty;
        public Color MarkerColor { get; set; } = Colors.Red;

        // 移除无效的隐式类型转换运算符声明
        // public static implicit operator MapMarker(MapMarker v)
        // {
        //     throw new NotImplementedException();
        // }
    }

    public enum MapMarkerType
    {
        UserLocation,
        Custom,
        CurrentLocation,
        Favorite
    }

    public class MapBounds
    {
        public double NorthLatitude { get; set; }
        public double SouthLatitude { get; set; }
        public double EastLongitude { get; set; }
        public double WestLongitude { get; set; }

        public double CenterLatitude => (NorthLatitude + SouthLatitude) / 2;
        public double CenterLongitude => (EastLongitude + WestLongitude) / 2;

        public double LatitudeDelta => Math.Abs(NorthLatitude - SouthLatitude);
        public double LongitudeDelta => Math.Abs(EastLongitude - WestLongitude);
    }
}