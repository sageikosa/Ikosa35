using System;
using System.Windows.Data;

namespace Uzi.Core
{
    [ValueConversion(typeof(OpenStatus), typeof(double))]
    public class OpenStatusDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is OpenStatus)
                return ((OpenStatus)value).Value;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
                return new OpenStatus(parameter, (double)value);
            else
                return new OpenStatus();
        }
    }
}
