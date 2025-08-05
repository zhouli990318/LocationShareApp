using System.Globalization;

namespace LocationShareApp.Helpers
{
    /// <summary>
    /// UTC时间转本地时间转换器
    /// </summary>
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // 如果DateTime的Kind是Unspecified，假设它是UTC时间
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                
                // 如果是UTC时间，转换为本地时间
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    return dateTime.ToLocalTime();
                }
                
                // 如果已经是本地时间，直接返回
                return dateTime;
            }
            
            return value ?? DateTime.Now;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // 转换回UTC时间
                if (dateTime.Kind == DateTimeKind.Local)
                {
                    return dateTime.ToUniversalTime();
                }
                
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                }
                
                return dateTime;
            }
            
            return value ?? DateTime.UtcNow;
        }
    }

    /// <summary>
    /// UTC时间转本地时间格式化转换器
    /// </summary>
    public class UtcToLocalTimeStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // 如果DateTime的Kind是Unspecified，假设它是UTC时间
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                
                // 转换为本地时间
                var localTime = dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime;
                
                // 使用参数指定的格式，如果没有参数则使用默认格式
                var format = parameter?.ToString() ?? "yyyy-MM-dd HH:mm:ss";
                return localTime.ToString(format, culture);
            }
            
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out var dateTime))
            {
                // 假设输入的是本地时间，转换为UTC
                return dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : 
                       DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
            }
            
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 相对时间转换器（如：2分钟前、1小时前等）
    /// </summary>
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // 如果DateTime的Kind是Unspecified，假设它是UTC时间
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                
                // 转换为本地时间
                var localTime = dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime;
                var now = DateTime.Now;
                var timeSpan = now - localTime;

                if (timeSpan.TotalMinutes < 1)
                {
                    return "刚刚";
                }
                else if (timeSpan.TotalMinutes < 60)
                {
                    return $"{(int)timeSpan.TotalMinutes}分钟前";
                }
                else if (timeSpan.TotalHours < 24)
                {
                    return $"{(int)timeSpan.TotalHours}小时前";
                }
                else if (timeSpan.TotalDays < 7)
                {
                    return $"{(int)timeSpan.TotalDays}天前";
                }
                else if (timeSpan.TotalDays < 30)
                {
                    return $"{(int)(timeSpan.TotalDays / 7)}周前";
                }
                else if (timeSpan.TotalDays < 365)
                {
                    return $"{(int)(timeSpan.TotalDays / 30)}个月前";
                }
                else
                {
                    return $"{(int)(timeSpan.TotalDays / 365)}年前";
                }
            }
            
            return "未知时间";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("RelativeTimeConverter不支持反向转换");
        }
    }
}