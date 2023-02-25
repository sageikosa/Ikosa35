using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class AxeBlade : FragmentPart
    {
        public AxeBlade(Func<Vector3D> origin)
            : base(origin)
        {
            HaftFactor = 0.8;
            IsFrontward = true;
        }

        public double Length { get; set; }
        public double Thickness { get; set; }
        public double BladeLength { get; set; }
        public double BladeHeight { get; set; }
        public double TopHeight { get; set; }
        public double BottomHeight { get; set; }
        public double TopPointOffset { get; set; }
        public double BottomPointOffset { get; set; }
        public double TopBackLength { get; set; }
        public double BottomBackLength { get; set; }
        public bool IsFrontward { get; set; }
        public double HaftFactor { get; set; }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return ElementFromModel(winfx, uzi, RenderModel() as GeometryModel3D, @"AxeBlade", null);
        }

        public override Model3D RenderModel()
        {
            // model points
            var _zBase = BladeHeight / 2;
            var _zTop = _zBase + TopHeight;
            var _zBottom = 0 - _zBase - BottomHeight;
            var _yMax = Length;
            var _yMiddle = (Length - BladeLength);
            var _xMax = Thickness / 2;

            var _pts = new Point3D[]
            { 
                // top point and front edge (0 to 4)
                new Point3D(0, _yMiddle + TopPointOffset, _zTop), new Point3D(0,_yMax, _zBase), new Point3D(-_xMax, _yMiddle, _zBase), new Point3D(0, _yMiddle-TopBackLength, 0), new Point3D(_xMax, _yMiddle, _zBase),
                // bottom point and front edge (5 to 9)
                new Point3D(0, _yMiddle + BottomPointOffset, _zBottom), new Point3D(0,_yMax, -_zBase), new Point3D(_xMax, _yMiddle, -_zBase), new Point3D(0, _yMiddle-BottomBackLength, 0), new Point3D(-_xMax, _yMiddle, -_zBase),
                // remaining points (10 to 13)
                new Point3D(_xMax,0,_zBase*HaftFactor), new Point3D(_xMax,0,-_zBase*HaftFactor), new Point3D(-_xMax,0,_zBase*HaftFactor), new Point3D(-_xMax,0,-_zBase*HaftFactor)
            };
            PointMover().Transform(_pts);
            if (!IsFrontward)
            {
                var _vect = Origin();
                var _origin = new Point3D(_vect.X, _vect.Y, _vect.Z);
                var _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180), _origin);
                _rotate.Transform(_pts);
            }

            // texture points
            Func<double, double, System.Windows.Point> _makePt = (x, y) => new System.Windows.Point(x, y);
            var _txc = new System.Windows.Point[]
            { 
                // top point
                _makePt(0,0), _makePt(0,0.33), _makePt(0.5,0.33), _makePt(0.66,0.33), _makePt(0.5,0.33),
                // bottom point
                _makePt(0,1), _makePt(0,0.66), _makePt(0.5,0.66), _makePt(0.66,0.66), _makePt(0.5,0.66),
                // back
                _makePt(1,0), _makePt(1,1), _makePt(1,0), _makePt(1,1)
            };

            // triangle indices
            var _tri = new int[]
            { 
                // top point
                0,1,2, 0,2,3, 0,3,4, 0,4,1, 
                // bottom point
                5,6,7, 5,7,8, 5,8,9, 5,9,6, 
                // edge
                1,4,6, 7,6,4, 1,6,2, 9,2,6,
                // sides
                12,2,13, 9,13,2, 10,11,4, 7,4,11,
                // top and bottom
                10,4,12, 2,12,4, 13,9,11, 7,11,9,
                // back
                10,12,11, 13,11,12
            };

            // mesh geometry
            var _mesh = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(_pts),
                TextureCoordinates = new System.Windows.Media.PointCollection(_txc),
                TriangleIndices = new System.Windows.Media.Int32Collection(_tri)
            };

            // geometry model
            return new GeometryModel3D(_mesh, null);
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Bottom":
                    return () => Origin() + new Vector3D(0, 0, -BladeHeight / 2);
                case @"Top":
                    return () => Origin() + new Vector3D(0, 0, BladeHeight / 2);
            }
            return base.ConnectionPoint(key);
        }
    }
}
