using Microsoft.Maui.Maps;
using LocationShareApp.Services;

namespace LocationShareApp.Helpers
{
    public static class MapHelper
    {
        /// <summary>
        /// 计算两个地理位置之间的距离（公里）
        /// </summary>
        public static double CalculateDistance(Location location1, Location location2)
        {
            const double earthRadiusKm = 6371.0;

            var lat1Rad = DegreesToRadians(location1.Latitude);
            var lat2Rad = DegreesToRadians(location2.Latitude);
            var deltaLatRad = DegreesToRadians(location2.Latitude - location1.Latitude);
            var deltaLonRad = DegreesToRadians(location2.Longitude - location1.Longitude);

            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return earthRadiusKm * c;
        }

        /// <summary>
        /// 计算方位角（度）
        /// </summary>
        public static double CalculateBearing(Location from, Location to)
        {
            var lat1Rad = DegreesToRadians(from.Latitude);
            var lat2Rad = DegreesToRadians(to.Latitude);
            var deltaLonRad = DegreesToRadians(to.Longitude - from.Longitude);

            var y = Math.Sin(deltaLonRad) * Math.Cos(lat2Rad);
            var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                    Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(deltaLonRad);

            var bearingRad = Math.Atan2(y, x);
            var bearingDeg = RadiansToDegrees(bearingRad);

            return (bearingDeg + 360) % 360;
        }

        /// <summary>
        /// 获取地图缩放级别建议
        /// </summary>
        public static double GetRecommendedZoomRadius(double distanceKm)
        {
            return distanceKm switch
            {
                <= 0.5 => 0.5,
                <= 1.0 => 1.0,
                <= 2.0 => 2.0,
                <= 5.0 => 5.0,
                <= 10.0 => 10.0,
                <= 20.0 => 20.0,
                _ => 50.0
            };
        }

        /// <summary>
        /// 创建包含所有位置点的地图区域
        /// </summary>
        public static MapSpan CreateSpanFromLocations(IEnumerable<Location> locations)
        {
            var locationList = locations.ToList();
            if (!locationList.Any())
            {
                // 默认返回上海区域
                return MapSpan.FromCenterAndRadius(new Location(31.2304, 121.4737), Distance.FromKilometers(10));
            }

            if (locationList.Count == 1)
            {
                return MapSpan.FromCenterAndRadius(locationList.First(), Distance.FromKilometers(1));
            }

            var minLat = locationList.Min(l => l.Latitude);
            var maxLat = locationList.Max(l => l.Latitude);
            var minLon = locationList.Min(l => l.Longitude);
            var maxLon = locationList.Max(l => l.Longitude);

            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;
            var center = new Location(centerLat, centerLon);

            var latDelta = Math.Abs(maxLat - minLat);
            var lonDelta = Math.Abs(maxLon - minLon);
            var maxDelta = Math.Max(latDelta, lonDelta);

            // 添加一些边距
            var radiusKm = Math.Max(1.0, maxDelta * 111.32 * 0.6); // 1度约等于111.32公里
            
            return MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(radiusKm));
        }

        /// <summary>
        /// 格式化距离显示
        /// </summary>
        public static string FormatDistance(double distanceKm)
        {
            if (distanceKm < 1.0)
            {
                return $"{(distanceKm * 1000):F0}m";
            }
            else if (distanceKm < 10.0)
            {
                return $"{distanceKm:F1}km";
            }
            else
            {
                return $"{distanceKm:F0}km";
            }
        }

        /// <summary>
        /// 格式化方位角显示
        /// </summary>
        public static string FormatBearing(double bearing)
        {
            var directions = new[]
            {
                "北", "东北偏北", "东北", "东北偏东",
                "东", "东南偏东", "东南", "东南偏南",
                "南", "西南偏南", "西南", "西南偏西",
                "西", "西北偏西", "西北", "西北偏北"
            };

            var index = (int)Math.Round(bearing / 22.5) % 16;
            return directions[index];
        }

        /// <summary>
        /// 检查位置是否在指定区域内
        /// </summary>
        public static bool IsLocationInRegion(Location location, Location center, double radiusKm)
        {
            var distance = CalculateDistance(location, center);
            return distance <= radiusKm;
        }

        /// <summary>
        /// 生成地图标记点的颜色
        /// </summary>
        public static Color GetPinColor(string category)
        {
            return category.ToLower() switch
            {
                "home" or "家" => Colors.Green,
                "work" or "工作" => Colors.Blue,
                "friend" or "朋友" => Colors.Orange,
                "restaurant" or "餐厅" => Colors.Red,
                "shopping" or "购物" => Colors.Purple,
                "hospital" or "医院" => Colors.Pink,
                "school" or "学校" => Colors.Yellow,
                _ => Colors.Gray
            };
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }

    /// <summary>
    /// 地图样式枚举
    /// </summary>
    public enum MapStyleType
    {
        Normal,     // 标准地图
        Satellite,  // 卫星地图
        Hybrid,     // 混合地图
        Terrain,    // 地形地图
        Night       // 夜间模式
    }

    /// <summary>
    /// 地图交通状况
    /// </summary>
    public enum TrafficStatus
    {
        Unknown,    // 未知
        Smooth,     // 畅通
        Slow,       // 缓慢
        Congested,  // 拥堵
        Blocked     // 严重拥堵
    }
}