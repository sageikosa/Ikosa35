using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Diagnostics;

namespace ModelMaker
{
    public class VolumetricMeshMaker
    {
        public static MeshGeometry3D Cylinder(double radius, double height, int sides, int planes)
            => CylindricalSurface(radius, height, sides, planes, 0, 360);

        #region public static MeshGeometry3D CylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        public static MeshGeometry3D CylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        {
            var _planeHeight = height / (planes - 1);
            var _textureVStep = 1d / (planes - 1d);
            var _textureHStep = 1d / sides;
            var _spin = 1d / (sides - 1d);
            var _mesh = new MeshGeometry3D();

            var _startAxisRotate = new AxisAngleRotation3D(new Vector3D(0, 0, 1), startAngle);
            var _startRotate = new RotateTransform3D(_startAxisRotate);

            var _axisRot = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (endAngle - startAngle) / sides);
            var _rotate = new RotateTransform3D(_axisRot);

            sides++;

            // add all points and normals
            for (var _planeIdx = 0; _planeIdx < planes; _planeIdx++)
            {
                // plane texture Y
                var _yTxt = 1d - (_textureVStep * _planeIdx);
                var _point = new Point3D(radius, 0, _planeHeight * _planeIdx);
                var _vect = new Vector3D(1, 0, 0);
                _point = _startRotate.Transform(_point);
                _vect = _startRotate.Transform(_vect);

                for (int _ptx = 0; _ptx < sides; _ptx++)
                {
                    // plane texture YX
                    var _xTxt = (_textureHStep * _ptx);
                    _mesh.Positions.Add(_point);
                    _mesh.Normals.Add(_vect);
                    _mesh.TextureCoordinates.Add(new Point(_xTxt, _yTxt));
                    _point = _rotate.Transform(_point);
                    _vect = _rotate.Transform(_vect);
                }
            }

            // synthesize all triangles
            for (var _triIdx = 0; _triIdx < sides - 1; _triIdx++)
            {
                for (var _planeIdx = 0; _planeIdx < planes - 1; _planeIdx++)
                {
                    _mesh.TriangleIndices.Add(_triIdx + (_planeIdx * sides));
                    _mesh.TriangleIndices.Add(_triIdx + 1 + (_planeIdx * sides));
                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * sides));

                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * sides));
                    _mesh.TriangleIndices.Add(_triIdx + 1 + (_planeIdx * sides));
                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * sides) + 1);
                }
            }
            return _mesh;
        }
        #endregion

        #region public static MeshGeometry3D PolygonalCylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        public static MeshGeometry3D PolygonalCylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        {
            var _planeHeight = height / (planes - 1);
            var _textureVStep = 1d / (planes - 1d);
            var _textureHStep = 1d / sides;
            var _mesh = new MeshGeometry3D();

            var _startAxisRotate = new AxisAngleRotation3D(new Vector3D(0, 0, 1), startAngle);
            var _startRotate = new RotateTransform3D(_startAxisRotate);

            var _axisRot = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (endAngle - startAngle) / sides);
            var _rotate = new RotateTransform3D(_axisRot);

            // add all points
            for (var _planeIdx = 0; _planeIdx < planes; _planeIdx++)
            {
                // plane texture Y
                var _yTxt = 1d - (_textureVStep * _planeIdx);
                var _point = new Point3D(radius, 0, _planeHeight * _planeIdx);
                _point = _startRotate.Transform(_point);

                for (var _ptx = 0; _ptx < sides + 1; _ptx++)
                {
                    // plane texture YX
                    var _xTxt = (_textureHStep * _ptx);
                    _mesh.Positions.Add(_point);
                    _mesh.TextureCoordinates.Add(new Point(_xTxt, _yTxt));
                    if ((_ptx != 0) && (_ptx != sides))
                    {
                        _mesh.Positions.Add(_point);
                        _mesh.TextureCoordinates.Add(new Point(_xTxt, _yTxt));
                    }

                    _point = _rotate.Transform(_point);
                }
            }

            // add all normals
            for (var _planeIdx = 0; _planeIdx < planes - 1; _planeIdx++)
            {
                for (var _ptx = 0; _ptx < sides; _ptx++)
                {
                    var _pt1 = _mesh.Positions[_planeIdx * sides * 2 + _ptx * 2];
                    var _pt2 = _mesh.Positions[_planeIdx * sides * 2 + _ptx * 2 + 1];
                    var _pt3 = _mesh.Positions[(_planeIdx + 1) * sides * 2 + _ptx * 2];
                    var _v1 = _pt2 - _pt1;
                    var _v2 = _pt3 - _pt1;
                    var _norm = Vector3D.CrossProduct(_v1, _v2);
                    _mesh.Normals.Add(_norm);
                    _mesh.Normals.Add(_norm);
                }
            }
            for (var _ptx = 0; _ptx < sides; _ptx++)
            {
                _mesh.Normals.Add(_mesh.Normals[(planes - 2) * sides * 2 + _ptx * 2]);
                _mesh.Normals.Add(_mesh.Normals[(planes - 2) * sides * 2 + _ptx * 2]);
            }

            // synthesize all triangles
            for (var _triIdx = 0; _triIdx < sides; _triIdx++)
            {
                for (var _planeIdx = 0; _planeIdx < planes - 1; _planeIdx += 2)
                {
                    _mesh.TriangleIndices.Add(_triIdx * 2 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + 1 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2));

                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + 1 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2) + 1);
                }
            }
            return _mesh;
        }
        #endregion

        #region public static MeshGeometry3D SegmentedCylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        public static MeshGeometry3D SegmentedCylindricalSurface(double radius, double height, int sides, int planes, double startAngle, double endAngle)
        {
            var _planeHeight = height / (planes - 1);
            var _textureVStep = 1d / (planes - 1d);
            var _mesh = new MeshGeometry3D();

            var _startAxisRotate = new AxisAngleRotation3D(new Vector3D(0, 0, 1), startAngle);
            var _startRotate = new RotateTransform3D(_startAxisRotate);

            var _axisRot = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (endAngle - startAngle) / sides);
            var _rotate = new RotateTransform3D(_axisRot);

            // add all points
            for (var _planeIdx = 0; _planeIdx < planes; _planeIdx++)
            {
                // plane texture Y
                var _yTxt = 1d - (_textureVStep * _planeIdx);
                var _point = new Point3D(radius, 0, _planeHeight * _planeIdx);
                _point = _startRotate.Transform(_point);

                for (var _ptx = 0; _ptx < sides + 1; _ptx++)
                {
                    _mesh.Positions.Add(_point);
                    if (_ptx == 0)
                    {
                        // initial point
                        _mesh.TextureCoordinates.Add(new Point(0, _yTxt));
                    }
                    else
                    {
                        // final point
                        _mesh.TextureCoordinates.Add(new Point(1, _yTxt));
                        if (_ptx != sides)
                        {
                            // next initial point
                            _mesh.Positions.Add(_point);
                            _mesh.TextureCoordinates.Add(new Point(0, _yTxt));
                        }
                    }

                    _point = _rotate.Transform(_point);
                }
            }

            // add all normals
            for (var _planeIdx = 0; _planeIdx < planes - 1; _planeIdx++)
            {
                for (var _ptx = 0; _ptx < sides; _ptx++)
                {
                    var _pt1 = _mesh.Positions[_planeIdx * sides * 2 + _ptx * 2];
                    var _pt2 = _mesh.Positions[_planeIdx * sides * 2 + _ptx * 2 + 1];
                    var _pt3 = _mesh.Positions[(_planeIdx + 1) * sides * 2 + _ptx * 2];
                    var _v1 = _pt2 - _pt1;
                    var _v2 = _pt3 - _pt1;
                    var _norm = Vector3D.CrossProduct(_v1, _v2);
                    _mesh.Normals.Add(_norm);
                    _mesh.Normals.Add(_norm);
                }
            }
            for (var _ptx = 0; _ptx < sides; _ptx++)
            {
                _mesh.Normals.Add(_mesh.Normals[(planes - 2) * sides * 2 + _ptx * 2]);
                _mesh.Normals.Add(_mesh.Normals[(planes - 2) * sides * 2 + _ptx * 2]);
            }

            // synthesize all triangles
            for (var _triIdx = 0; _triIdx < sides; _triIdx++)
            {
                for (var _planeIdx = 0; _planeIdx < planes - 1; _planeIdx += 2)
                {
                    _mesh.TriangleIndices.Add(_triIdx * 2 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + 1 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2));

                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + 1 + (_planeIdx * sides * 2));
                    _mesh.TriangleIndices.Add(_triIdx * 2 + ((_planeIdx + 1) * sides * 2) + 1);
                }
            }
            return _mesh;
        }
        #endregion

        public static MeshGeometry3D Sphere(double radius, int longSides, int latSides)
            => SphericalSurface(radius, longSides, latSides, 0, 360, -90, 90);

        #region public static MeshGeometry3D SphericalSurface(double radius, int longSides, int latSides, double longAngleA, double longAngleB, double latAngleA, double latAngleB)
        public static MeshGeometry3D SphericalSurface(double radius, int longSides, int latSides, double longAngleA, double longAngleB,
            double latAngleA, double latAngleB)
        {
            MeshGeometry3D _mesh = new MeshGeometry3D();
            double _longSides = longSides;
            double _latSides = latSides;

            // starting longitudinal rotation
            AxisAngleRotation3D _longAxisStart = new AxisAngleRotation3D(new Vector3D(0, 0, 1), longAngleA);
            RotateTransform3D _longStartRotate = new RotateTransform3D(_longAxisStart);

            // sweeping longitudinal rotation
            AxisAngleRotation3D _longSweep = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (longAngleB - longAngleA) / _longSides);
            RotateTransform3D _longRotate = new RotateTransform3D(_longSweep);
            double _longStep = (longAngleB - longAngleA) / _longSides;
            longSides++;

            double _latStep = (latAngleB - latAngleA) / _latSides;
            latSides++;

            // points and normals
            int _ptCount = 0;
            double _latitude = latAngleA;
            for (int _ltx = 0; _ltx < latSides; _ltx++)
            {
                Point3D _point = new Point3D(radius, 0, 0);
                Vector3D _vect = new Vector3D(1, 0, 0);

                // inclination/declination
                AxisAngleRotation3D _latAxisStart = new AxisAngleRotation3D(new Vector3D(0, -1, 0), _latitude);
                RotateTransform3D _latRotate = new RotateTransform3D(_latAxisStart);
                _point = _latRotate.Transform(_point);
                _vect = _latRotate.Transform(_vect);

                // longitude
                _point = _longStartRotate.Transform(_point);
                _vect = _longStartRotate.Transform(_vect);

                double _longitude = longAngleA;
                for (int _lgx = 0; _lgx < longSides; _lgx++)
                {
                    _ptCount++;
                    _mesh.Positions.Add(_point);
                    _mesh.Normals.Add(_vect);
                    // TODO: texture coordinates
                    _point = _longRotate.Transform(_point);
                    _vect = _longRotate.Transform(_vect);
                    _longitude += _longStep;
                }
                _latitude += _latStep;
            }

            // triangles
            int _triCount = 0;
            for (int _triIdx = 0; _triIdx < longSides - 1; _triIdx++)
            {
                for (int _planeIdx = 0; _planeIdx < latSides - 1; _planeIdx++)
                {
                    _triCount++;
                    _mesh.TriangleIndices.Add(_triIdx + (_planeIdx * longSides));
                    _mesh.TriangleIndices.Add(_triIdx + 1 + (_planeIdx * longSides));
                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * longSides));

                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * longSides));
                    _mesh.TriangleIndices.Add(_triIdx + 1 + (_planeIdx * longSides));
                    _mesh.TriangleIndices.Add(_triIdx + ((_planeIdx + 1) * longSides) + 1);
                }
            }
            return _mesh;
        }
        #endregion

        #region public static MeshGeometry3D ConicSurface(double lowRadius, double highRadius, double height, double startAngle, double endAngle, int sides, int levels)
        public static MeshGeometry3D ConicSurface(double lowRadius, double highRadius, double height, double startAngle, double endAngle, int sides, int levels)
        {
            MeshGeometry3D _mesh = new MeshGeometry3D();
            double _rise = height;
            double _stepRise = _rise / levels;
            double _run = highRadius - lowRadius;
            double _stepRun = _run / levels;
            Vector3D _startVector = Vector3D.CrossProduct(new Vector3D(0, 1, 0), new Vector3D(_rise, 0, _rise));
            int _sides = sides + 1;

            AxisAngleRotation3D _axisSpin = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (endAngle - startAngle) / sides);
            RotateTransform3D _spin = new RotateTransform3D(_axisSpin);

            AxisAngleRotation3D _startSpin = new AxisAngleRotation3D(new Vector3D(0, 0, 1), startAngle);
            RotateTransform3D _spinInit = new RotateTransform3D(_startSpin);

            for (int _px = 0; _px <= levels; _px++)
            {
                // start point and vector
                Point3D _point = new Point3D(lowRadius + (_stepRun * _px), 0, _stepRise * _px);
                Vector3D _vect = _startVector;
                _point = _spinInit.Transform(_point);
                _vect = _spinInit.Transform(_vect);

                for (int _sx = 0; _sx <= sides; _sx++)
                {
                    _mesh.Positions.Add(_point);
                    _mesh.Normals.Add(_vect);

                    // next point and vector
                    _point = _spin.Transform(_point);
                    _vect = _spin.Transform(_vect);
                }
            }

            // triangles
            for (int _px = 0; _px < levels; _px++)
            {
                for (int _sx = 0; _sx < sides; _sx++)
                {
                    _mesh.TriangleIndices.Add(_sx + (_px * _sides));
                    _mesh.TriangleIndices.Add(_sx + (_px * _sides) + 1);
                    _mesh.TriangleIndices.Add(_sx + ((_px + 1) * _sides));

                    _mesh.TriangleIndices.Add(_sx + (_px * _sides) + 1);
                    _mesh.TriangleIndices.Add(_sx + ((_px + 1) * _sides) + 1);
                    _mesh.TriangleIndices.Add(_sx + ((_px + 1) * _sides));
                }
            }

            return _mesh;
        }
        #endregion
    }
}
