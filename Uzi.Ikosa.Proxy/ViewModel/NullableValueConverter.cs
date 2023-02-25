using System;
using System.Globalization;
using System.Windows.Data;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class NullableValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return null;

            return value;
        }

        #endregion
    }
}
