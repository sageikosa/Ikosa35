using System.Windows.Markup;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(MeshGeometry3D))]
    public class CylinderMesh : MeshExtension
    {
        public CylinderMesh()
        {
            Diameter = 1d;
            ThetaDiv = 20;
        }

        public Point3D P1 { get; set; }
        public Point3D P2 { get; set; }
        public double Diameter { get; set; }
        public int ThetaDiv { get; set; }
        public bool BaseCap { get; set; }
        public bool TopCap { get; set; }
        public double Rotation { get; set; }

        protected override MeshGeometry3D GenerateMesh()
        {
            var _builder = new MeshBuilder();
            if (Rotation != 0)
            {
                var _rotation = new RotateTransform3D(new AxisAngleRotation3D(P2 - P1, Rotation), P1);
                _builder.AddCylinder(P1, P2, Diameter, BaseCap, TopCap, ThetaDiv, _rotation, _rotation);
            }
            else
            {
                _builder.AddCylinder(P1, P2, Diameter, BaseCap, TopCap, ThetaDiv);
            }
            return _builder.ToMesh();
        }
    }
}
