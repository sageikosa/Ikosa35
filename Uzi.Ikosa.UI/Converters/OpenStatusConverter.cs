using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Uzi.Core;

namespace Uzi.Ikosa.UI
{
    [ValueConversion(typeof(OpenStatus), typeof(double))]
    public class OpenStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((OpenStatus)value).Value;
            }
            catch
            {
                return 0d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new OpenStatus((double)value);
            }
            catch
            {
            }
            return default(OpenStatus);
        }

        private static readonly OpenStatusConverter _Static = new OpenStatusConverter();
        public static OpenStatusConverter Static => _Static;
    }
}
