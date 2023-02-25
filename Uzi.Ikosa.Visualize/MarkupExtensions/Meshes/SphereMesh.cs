using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(MeshGeometry3D))]
    public class SphereMesh : MeshExtension
    {
        public SphereMesh()
        {
            Radius = 1d;
            ThetaDiv = 20;
            PhiDiv = 10;
        }

        public Point3D Center { get; set; }
        public double Radius { get; set; }
        public int ThetaDiv { get; set; }
        public int PhiDiv { get; set; }

        protected override MeshGeometry3D GenerateMesh()
        {
            var _builder = new MeshBuilder();
            _builder.AddSphere(Center, Radius, ThetaDiv, PhiDiv);
            return _builder.ToMesh();
        }
    }
}
