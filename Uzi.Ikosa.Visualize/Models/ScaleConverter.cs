using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Uzi.Visualize
{
    public class ScaleConverter : IValueConverter
    {
        private static ScaleConverter _Factor90 = new ScaleConverter { Factor = 90d };
        public static ScaleConverter Factor90 { get { return _Factor90; } }

        private static ScaleConverter _Factor180 = new ScaleConverter { Factor = 180d };
        public static ScaleConverter Factor180 { get { return _Factor180; } }

        private static ScaleConverter _Factor270 = new ScaleConverter { Factor = 270d };
        public static ScaleConverter Factor270 { get { return _Factor270; } }

        public double Factor { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) * Factor;
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
                return System.Convert.ToDouble(value) / Factor;
            }
            catch
            {
                return 0d;
            }
        }

        #endregion
    }
}
