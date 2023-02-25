using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ModelMaker
{
    public class FlatMeshMaker
    {
        public static MeshGeometry3D Polygon(double radius, int sides, double refAngle)
        {
            return PolygonalSurface(radius, sides, refAngle, refAngle + 360);
        }

        #region public static MeshGeometry3D PolygonalSurface(double radius, int sides, double startAngle, double endAngle)
        public static MeshGeometry3D PolygonalSurface(double radius, int sides, double startAngle, double endAngle)
        {
            MeshGeometry3D _mesh = new MeshGeometry3D();
            Point3D _point = new Point3D(radius, 0, 0);
            Vector3D _vect = new Vector3D(0, 0, 1);
            double _sides = sides;

            // start position
            AxisAngleRotation3D _startSpin = new AxisAngleRotation3D(_vect, startAngle);
            _point = (new RotateTransform3D(_startSpin)).Transform(_point);

            // steps
            AxisAngleRotation3D _axisSpin = new AxisAngleRotation3D(_vect, (endAngle - startAngle) / _sides);
            RotateTransform3D _spin = new RotateTransform3D(_axisSpin);

            // center point
            _mesh.Positions.Add(new Point3D());
            _mesh.Normals.Add(_vect);

            // points
            for (int _sx = 0; _sx <= sides; _sx++)
            {
                _mesh.Positions.Add(_point);
                _mesh.Normals.Add(_vect);
                _point = _spin.Transform(_point);
            }

            // triangles
            for (int _sx = 0; _sx < sides; _sx++)
            {
                _mesh.TriangleIndices.Add(0);
                _mesh.TriangleIndices.Add(_sx + 1);
                _mesh.TriangleIndices.Add(_sx + 2);
            }

            return _mesh;
        }
        #endregion

        #region public static MeshGeometry3D RingSurface(double innerRadius, double outerRadius, int sides, int rings, double startAngle, double endAngle)
        public static MeshGeometry3D RingSurface(double innerRadius, double outerRadius, int sides, int rings, double startAngle, double endAngle)
        {
            MeshGeometry3D _mesh = new MeshGeometry3D();
            Vector3D _vect = new Vector3D(0, 0, 1);
            double _rings = rings;
            double _ringStep = (outerRadius - innerRadius) / _rings;
            double _sides = sides;
            int _ringPoints = sides+1;

            // starting angle
            AxisAngleRotation3D _startSpin = new AxisAngleRotation3D(_vect, startAngle);
            RotateTransform3D _startAngle = new RotateTransform3D(_startSpin);

            // spin steps
            AxisAngleRotation3D _axisSpin = new AxisAngleRotation3D(_vect, (endAngle - startAngle) / _sides);
            RotateTransform3D _spin = new RotateTransform3D(_axisSpin);

            // points and normals
            for (int _rx = 0; _rx <= rings; _rx++)
            {
                // starting point
                Point3D _point = new Point3D(innerRadius + _ringStep * _rx, 0, 0);
                _point = _startAngle.Transform(_point);

                // create a band of points
                for (int _sx = 0; _sx <= sides; _sx++)
                {
                    _mesh.Positions.Add(_point);
                    _mesh.Normals.Add(_vect);
                    _point = _spin.Transform(_point);
                }
            }

            // triangles
            for (int _rx = 0; _rx < rings; _rx++)
            {
                for (int _sx = 0; _sx < sides; _sx++)
                {
                    _mesh.TriangleIndices.Add(_sx + (_rx * _ringPoints));
                    _mesh.TriangleIndices.Add(_sx + ((_rx + 1) * _ringPoints));
                    _mesh.TriangleIndices.Add(_sx + (_rx * _ringPoints) + 1);

                    _mesh.TriangleIndices.Add(_sx + ((_rx + 1) * _ringPoints));
                    _mesh.TriangleIndices.Add(_sx + ((_rx + 1) * _ringPoints) + 1);
                    _mesh.TriangleIndices.Add(_sx + (_rx * _ringPoints) + 1);
                }
            }

            return _mesh;
        }
        #endregion
    }
}
