using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CharacterModeler
{
    public class ValueInverter : IValueConverter
    {
        private static ValueInverter _Static = new ValueInverter();
        public static ValueInverter Static { get { return _Static; } }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) * -1d;
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
                return System.Convert.ToDouble(value) / -1d;
            }
            catch
            {
                return 0d;
            }
        }

        #endregion
    }
}
