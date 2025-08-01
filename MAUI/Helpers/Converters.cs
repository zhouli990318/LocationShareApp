using System.Globalization;

namespace LocationShareApp.Helpers
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                // 处理 null 值
                if (value == null)
                    return true; // !false = true

                // 处理 bool 类型
                if (value is bool boolValue)
                    return !boolValue;

                // 处理字符串类型
                if (value is string stringValue)
                {
                    if (bool.TryParse(stringValue, out bool parsedBool))
                        return !parsedBool;
                    
                    // 字符串非空视为 true
                    return string.IsNullOrEmpty(stringValue);
                }

                // 处理数字类型
                if (value is int intValue)
                    return intValue == 0; // 0 为 false，非0 为 true，所以取反

                if (value is double doubleValue)
                    return Math.Abs(doubleValue) < double.Epsilon; // 接近0为false

                if (value is float floatValue)
                    return Math.Abs(floatValue) < float.Epsilon;

                // 处理其他类型 - 非null对象视为true
                return false; // !true = false
            }
            catch (Exception ex)
            {
                // 记录异常并返回安全默认值
                System.Diagnostics.Debug.WriteLine($"InvertedBoolConverter.Convert异常: {ex.Message}, 输入值: {value}, 类型: {value?.GetType()}");
                return true; // 默认返回true（相当于输入为false的反转）
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                // 处理 null 值
                if (value == null)
                    return false;

                // 处理 bool 类型
                if (value is bool boolValue)
                    return !boolValue;

                // 处理字符串类型
                if (value is string stringValue)
                {
                    if (bool.TryParse(stringValue, out bool parsedBool))
                        return !parsedBool;
                    
                    return string.IsNullOrEmpty(stringValue);
                }

                // 处理数字类型
                if (value is int intValue)
                    return intValue == 0;

                if (value is double doubleValue)
                    return Math.Abs(doubleValue) < double.Epsilon;

                if (value is float floatValue)
                    return Math.Abs(floatValue) < float.Epsilon;

                // 其他类型默认处理
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InvertedBoolConverter.ConvertBack异常: {ex.Message}, 输入值: {value}, 类型: {value?.GetType()}");
                return false; // 默认返回false
            }
        }
    }
    
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isOnline = (bool)(value ?? false);
            return isOnline ? Colors.Green : Colors.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}