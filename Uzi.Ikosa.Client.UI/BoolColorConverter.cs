using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Uzi.Ikosa.Client.UI
{
    [ValueConversion(typeof(bool), typeof(Color))]
    public class BoolColorConverter : IValueConverter
    {
        public BoolColorConverter()
        {
            TrueColor = Colors.Cyan;
            FalseColor = Colors.Transparent;
        }

        public Color TrueColor { get; set; }
        public Color FalseColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _bool = (bool)value;
            if (targetType == typeof(Color))
            {
                return _bool ? TrueColor : FalseColor;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
            {
                var _color = (Color)value;
                return (_color == TrueColor);
            }
            return false;
        }
    }
}
