using System;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Point3D))]
    public class Point3DVal : MarkupExtension
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Point3D(X, Y, Z);
        }
    }
}
