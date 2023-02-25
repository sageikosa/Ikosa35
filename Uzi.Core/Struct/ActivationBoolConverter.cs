using System;
using System.Windows.Data;

namespace Uzi.Core
{
    [ValueConversion(typeof(Activation), typeof(bool))]
    public class ActivationBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Activation)
                return ((Activation)value).IsActive;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return new Activation(parameter, (bool)value);
            else
                return new Activation();
        }
    }
}
