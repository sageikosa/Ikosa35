using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Uzi.Ikosa.Tactical;
using System.Windows;

namespace Uzi.Ikosa.Workshop
{
    public class CellMaterialSelector : DataTemplateSelector
    {
        public DataTemplate LiquidTemplate { get; set; }
        public DataTemplate SolidTemplate { get; set; }
        public DataTemplate GasTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is LiquidCellMaterial)
            {
                return LiquidTemplate;
            }
            else if (item is SolidCellMaterial)
            {
                return SolidTemplate;
            }
            else if (item is GasCellMaterial)
            {
                return GasTemplate;
            }
            return null;
        }
    }
}
