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
    public class EllipticalTubeMesh : MeshExtension
    {
        public EllipticalTubeMesh()
        {
        }

        /// <summary>Center of curve, from which points will be projected</summary>
        public Point3D Center { get; set; }

        /// <summary>Direction indicating axis around which points will be distributed</summary>
        public Vector3D PolarAxis { get; set; }

        /// <summary>Rotation before adjusting the polar axis</summary>
        public double PrimaryAngle { get; set; }

        /// <summary>Total Arc Angle</summary>
        public double AngularSpread { get; set; }

        /// <summary>Distance for Left and Right radius of curve (assuming a semi-circular arc)</summary>
        public double LateralRadius { get; set; }

        /// <summary>Distance for Primary radius of curve (assuming a semi-circular arc)</summary>
        public double PrimaryRadius { get; set; }

        /// <summary>Thickness of the tube at the ends</summary>
        public double LateralTubeThickness { get; set; }

        /// <summary>Thickness of the tube at the &quot;peak&quot;</summary>
        public double PrimaryTubeThickness { get; set; }

        /// <summary>Number of projected center points between left and right angles</summary>
        public int CurveSegments { get; set; }

        /// <summary>Number of sides of the tube</summary>
        public int ThetaDiv { get; set; }

        protected override MeshGeometry3D GenerateMesh()
        {
            var _builder = new MeshBuilder();
            var _segments = CurveSegments > 1 ? CurveSegments : 2;

            // project all points onto an ideal circular arc, and distribute diameter and x-texture mappings
            var _pts = new List<Point3D>();
            var _thick = new List<double>();

            // texture
            var _texture = new List<double>();
            var _textureIncr = 1d / (double)_segments;
            var _textureVal = 0d;

            // setup angular steps
            var _radianSpread = AngularSpread * Math.PI / 180d;
            var _curveIncr = _radianSpread / (double)_segments;
            var _halfCurve = _radianSpread / 2d;
            for (var _heading = -_halfCurve; _heading <= _halfCurve; _heading += _curveIncr)
            {
                // trig functions on unit circle
                _pts.Add(new Point3D(Math.Cos(_heading), Math.Sin(_heading), 0));

                // linear interpolation of thickness
                _thick.Add(PrimaryTubeThickness + (LateralTubeThickness - PrimaryTubeThickness) * Math.Abs(_heading/_halfCurve));

                // texture coordinates
                _texture.Add(_textureVal);
                _textureVal += _textureIncr;
            }

            // zAxis
            var _zAxis = new Vector3D(0,0,1);

            // axis used to rotate the zAxis to the PolarAxis
            var _polarRotater = Vector3D.CrossProduct(_zAxis, PolarAxis);

            // angle used to rotate the zAxis to the PolarAxis
            var _polarAdjustment = _polarRotater.AxisAngleBetween(_zAxis, PolarAxis);

            // scale arc, rotate to primary angle, adjust the polar axis, then adjust the center
            var _txGroup = new Transform3DGroup();
            _txGroup.Children.Add(new ScaleTransform3D(PrimaryRadius, LateralRadius, 0));
            _txGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(_zAxis, PrimaryAngle), new Point3D()));
            _txGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(_polarRotater, _polarAdjustment), new Point3D()));
            _txGroup.Children.Add(new TranslateTransform3D(Center.ToVector3D()));
            _txGroup.Freeze();

            // use as a single matrix transform (probably already did this internally, but I like to be thorough)
            var _matrix = new MatrixTransform3D(_txGroup.Value);
            _matrix.Freeze();

            // transform point array
            var _finalPts = _pts.ToArray();
            _matrix.Transform(_finalPts);

            // add tube with parameters
            _builder.AddTube(_finalPts, _texture.ToArray(), _thick.ToArray(), ThetaDiv, false);

            // return
            return _builder.ToMesh();
        }
    }
}
