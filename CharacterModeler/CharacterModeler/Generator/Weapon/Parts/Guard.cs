using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Guard : FragmentPart
    {
        public Guard(Func<Vector3D> origin) : base(origin) { }

        public double Width { get; set; }
        public double Thickness { get; set; }
        public double Length { get; set; }
        public bool IsInverted { get; set; }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return ElementFromModel(winfx, uzi, RenderModel() as GeometryModel3D, @"Guard", null);
        }

        public override Model3D RenderModel()
        {
            var _x = Width / 2;
            var _y = Thickness / 2;
            var _z = IsInverted ? -Length : Length;
            var _pts = new Point3D[] 
            { 
                new Point3D(_x, 0, 0),   new Point3D(0, _y,  0),   new Point3D(-_x, 0, 0),
                new Point3D(_x, 0, _z),  new Point3D(0, _y,  _z),  new Point3D(-_x, 0, _z) ,
                new Point3D(-_x, 0, 0),  new Point3D(0, -_y,  0),  new Point3D(_x, 0, 0),
                new Point3D(-_x, 0, _z), new Point3D(0, -_y,  _z), new Point3D(_x, 0, _z) ,
            };
            PointMover().Transform(_pts);
            var _txc = new System.Windows.Point[]
            {
                new System.Windows.Point(0,1), new System.Windows.Point(0.5,1), new System.Windows.Point(1,1),
                new System.Windows.Point(0,0), new System.Windows.Point(0.5,0), new System.Windows.Point(1,0),
                new System.Windows.Point(0,1), new System.Windows.Point(0.5,1), new System.Windows.Point(1,1),
                new System.Windows.Point(0,0), new System.Windows.Point(0.5,0), new System.Windows.Point(1,0)
            };
            var _tri = new int[] { };
            if (IsInverted)
                _tri = new int[] { 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 6, 9, 7, 7, 9, 10, 7, 10, 8, 8, 10, 11, 3, 5, 4, 9, 11, 10, 0, 1, 2, 6, 7, 8 };
            else
                _tri = new int[] { 0, 1, 3, 1, 4, 3, 1, 2, 4, 2, 5, 4, 6, 7, 9, 7, 10, 9, 7, 8, 10, 8, 11, 10, 3, 4, 5, 9, 10, 11, 0, 2, 1, 6, 8, 7 };
            var _mesh = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(_pts),
                TextureCoordinates = new System.Windows.Media.PointCollection(_txc),
                TriangleIndices = new System.Windows.Media.Int32Collection(_tri)
            };
            return new GeometryModel3D(_mesh, null);
        }
    }
}
