using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public class ColorBrushVal : MarkupExtension
    {
        public string Key { get; set; }

        public string Color { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ColorVal._KeyedColors ??= new Dictionary<string, string>();
            if (ColorVal._KeyedColors.TryGetValue(Key, out var _lookup))
            {
                return new BrushConverter().ConvertFromString(_lookup) as Brush;
            }

            // add since there wasn't already a value
            ColorVal._KeyedColors[Key] = Color;
            return new BrushConverter().ConvertFromString(Color) as Brush;
        }
    }
}
