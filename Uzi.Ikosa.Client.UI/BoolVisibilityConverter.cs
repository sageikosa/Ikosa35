using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Uzi.Ikosa.Client.UI
{

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolVisibilityConverter : IValueConverter
    {
        public BoolVisibilityConverter()
        {
            TrueMode = Visibility.Visible;
            FalseMode = Visibility.Collapsed;
        }

        public Visibility TrueMode { get; set; }
        public Visibility FalseMode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _bool = (bool)value;
            if (targetType == typeof(Visibility))
            {
                return _bool ? TrueMode : FalseMode;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                var _vis = (Visibility)value;
                return (_vis == TrueMode);
            }
            return false;
        }

    }
}
