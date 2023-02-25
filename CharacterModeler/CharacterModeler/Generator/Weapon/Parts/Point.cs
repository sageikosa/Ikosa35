using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Point : FragmentPart
    {
        public Point(Func<Vector3D> origin) : base(origin) { }

        public IBottomDimensionProvider Bottom { get; set; }
        public double Length { get; set; }
        public bool IsInverted { get; set; }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return ElementFromModel(winfx, uzi, RenderModel() as GeometryModel3D, @"Point", null);
        }

        public override Model3D RenderModel()
        {
            var _x = Bottom.Width / 2;
            var _y = Bottom.Thickness / 2;
            var _z = IsInverted ? -Length : Length;
            var _pts = new Point3D[] 
            { 
                new Point3D(_x, 0, 0), new Point3D(0,_y, 0), new Point3D(-_x, 0, 0),
                new Point3D(0, 0, _z), new Point3D(0, 0, _z), 
                new Point3D(-_x, 0, 0), new Point3D(0,-_y, 0), new Point3D(_x, 0, 0),
                new Point3D(0, 0, _z), new Point3D(0, 0, _z)
            };
            PointMover().Transform(_pts);
            var _txc = new System.Windows.Point[]
            {
                new System.Windows.Point(0,1), new System.Windows.Point(0.5,1), new System.Windows.Point(1,1),
                new System.Windows.Point(0,0), new System.Windows.Point(1,0),
                new System.Windows.Point(0,1), new System.Windows.Point(0.5,1), new System.Windows.Point(1,1),
                new System.Windows.Point(0,0), new System.Windows.Point(1,0)
            };
            var _tri = new int[] { };
            if (IsInverted)
                _tri = new int[] { 0, 3, 1,  1, 4, 2,  5, 8, 6,  6, 9, 7 };
            else
                _tri = new int[] { 0, 1, 3,  1, 2, 4,  5, 6, 8,  6, 7, 9 };
            var _mesh = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(_pts),
                TextureCoordinates = new System.Windows.Media.PointCollection(_txc),
                TriangleIndices = new System.Windows.Media.Int32Collection(_tri)
            };
            return new GeometryModel3D(_mesh, null);
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Top":
                    return () => Origin() + new Vector3D(0, 0, Length);
            }
            return base.ConnectionPoint(key);
        }
    }
}
