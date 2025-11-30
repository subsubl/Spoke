using System;
using Microsoft.Maui.Controls;

namespace IxiHome.Converters
{
    public class IsNotMinDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is DateTime dt)
                {
                    return dt != DateTime.MinValue;
                }
            }
            catch { }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
