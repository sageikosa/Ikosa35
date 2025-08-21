using System;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Double))]
    public class DoubleSum : MarkupExtension
    {
        public double A { get; set; }
        public double B { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return A + B;
        }
    }
}