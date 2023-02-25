using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CharacterModeler
{
    public class CenterReflector : IValueConverter
    {
        private static CenterReflector _Reflector = new CenterReflector();
        public static CenterReflector Static { get { return _Reflector; } }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return 96d - System.Convert.ToDouble(value);
            }
            catch
            {
                return 0d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) - 96d;
            }
            catch
            {
                return 0d;
            }
        }

        #endregion
    }
}
