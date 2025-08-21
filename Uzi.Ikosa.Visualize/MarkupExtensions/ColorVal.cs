using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Color))]
    public class ColorVal : MarkupExtension
    {
        [ThreadStatic]
        internal static IDictionary<string, string> _KeyedColors;

        public static IDictionary<string, string> GetKeyedColors()
            => _KeyedColors;

        public static void SetKeyedColors(IDictionary<string, string> keyedColors)
            => _KeyedColors = keyedColors;

        public string Key { get; set; }

        public string Color { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _KeyedColors ??= new Dictionary<string, string>();
            if (_KeyedColors.TryGetValue(Key, out var _lookup))
            {
                return (Color)ColorConverter.ConvertFromString(_lookup);
            }

            // add since there wasn't already a value
            _KeyedColors[Key] = Color;
            return (Color)ColorConverter.ConvertFromString(Color);
        }
    }
}
