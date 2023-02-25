using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Infos;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MenuViewModelStyleSelector : StyleSelector
    {
        public Style IconMenu { get; set; }
        public Style NoIconMenu { get; set; }
        public Style Separator { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SeparatorViewModel)
            {
                return Separator ?? NoIconMenu;
            }
            if ((item as MenuViewModel)?.IconSource is IIconInfo _iconInfo
                && (_iconInfo?.Icon != null)
                && (_iconInfo.IconResolver != null))
            {
                return IconMenu ?? NoIconMenu;
            }
            return NoIconMenu;
        }
    }
}
