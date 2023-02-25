using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    [ValueConversion(typeof(System.Collections.IList), typeof(IEnumerable))]
    public class CollectionViewSorter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _list = value as IList;
            var _view = new ListCollectionView(_list);
            var _sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);
            _view.SortDescriptions.Add(_sort);
            return _view;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
