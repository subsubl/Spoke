using System.Globalization;

namespace IxiHome.Utils;

/// <summary>
/// Converts boolean to Color for visual feedback
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public Color? TrueColor { get; set; }
    public Color? FalseColor { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts entity state to display string
/// </summary>
public class StateToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string state)
        {
            return state.ToUpper() switch
            {
                "ON" => "On",
                "OFF" => "Off",
                "UNAVAILABLE" => "Unavailable",
                "UNKNOWN" => "Unknown",
                _ => state
            };
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DateTime to relative time string
/// </summary>
public class DateTimeToRelativeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalSeconds < 60)
                return "Just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            else
                return $"{(int)timeSpan.TotalDays}d ago";
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts boolean value
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
}
