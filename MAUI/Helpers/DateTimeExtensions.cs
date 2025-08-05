namespace LocationShareApp.Helpers
{
    /// <summary>
    /// DateTime扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 将UTC时间转换为本地时间
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>本地时间</returns>
        public static DateTime ToLocalTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            
            return utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime.ToLocalTime() : utcDateTime;
        }

        /// <summary>
        /// 将本地时间转换为UTC时间
        /// </summary>
        /// <param name="localDateTime">本地时间</param>
        /// <returns>UTC时间</returns>
        public static DateTime ToUtcTime(this DateTime localDateTime)
        {
            if (localDateTime.Kind == DateTimeKind.Unspecified)
            {
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
            }
            
            return localDateTime.Kind == DateTimeKind.Local ? localDateTime.ToUniversalTime() : localDateTime;
        }

        /// <summary>
        /// 格式化显示本地时间
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <param name="format">格式字符串，默认为"yyyy-MM-dd HH:mm:ss"</param>
        /// <returns>格式化的本地时间字符串</returns>
        public static string ToLocalTimeString(this DateTime utcDateTime, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return utcDateTime.ToLocalTime().ToString(format);
        }

        /// <summary>
        /// 获取相对时间描述（如：2分钟前、1小时前等）
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>相对时间描述</returns>
        public static string ToRelativeTimeString(this DateTime utcDateTime)
        {
            var localTime = utcDateTime.ToLocalTime();
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

        /// <summary>
        /// 判断时间是否为今天
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>是否为今天</returns>
        public static bool IsToday(this DateTime utcDateTime)
        {
            var localTime = utcDateTime.ToLocalTime();
            return localTime.Date == DateTime.Today;
        }

        /// <summary>
        /// 判断时间是否为昨天
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>是否为昨天</returns>
        public static bool IsYesterday(this DateTime utcDateTime)
        {
            var localTime = utcDateTime.ToLocalTime();
            return localTime.Date == DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// 获取友好的时间显示格式
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>友好的时间显示</returns>
        public static string ToFriendlyString(this DateTime utcDateTime)
        {
            var localTime = utcDateTime.ToLocalTime();
            
            if (localTime.IsToday())
            {
                return $"今天 {localTime:HH:mm}";
            }
            else if (localTime.IsYesterday())
            {
                return $"昨天 {localTime:HH:mm}";
            }
            else if ((DateTime.Now - localTime).TotalDays < 7)
            {
                var dayOfWeek = localTime.DayOfWeek switch
                {
                    DayOfWeek.Monday => "周一",
                    DayOfWeek.Tuesday => "周二",
                    DayOfWeek.Wednesday => "周三",
                    DayOfWeek.Thursday => "周四",
                    DayOfWeek.Friday => "周五",
                    DayOfWeek.Saturday => "周六",
                    DayOfWeek.Sunday => "周日",
                    _ => localTime.ToString("MM-dd")
                };
                return $"{dayOfWeek} {localTime:HH:mm}";
            }
            else if (localTime.Year == DateTime.Now.Year)
            {
                return localTime.ToString("MM-dd HH:mm");
            }
            else
            {
                return localTime.ToString("yyyy-MM-dd HH:mm");
            }
        }
    }
}