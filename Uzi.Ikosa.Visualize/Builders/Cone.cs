using System;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    public class Cone : Sphere
    {
        #region Construction
        public Cone(ICellLocation origin, LocationAimMode locMode, int lengthInCells, ICellLocation target)
            : base(origin, locMode, lengthInCells)
        {
            _Target = target;
            _CenterLine = _LocMode.GetPoint3D(_Target) - _LocMode.GetPoint3D(_Origin);
            DetermineAltOrigin();
        }
        #endregion

        #region private void DetermineAltOrigin()
        private void DetermineAltOrigin()
        {
            // find angles to 8 bordering cells
            var _minAngle = 180d;
            for (var _z = _Origin.Z - 1; _z <= _Origin.Z; _z++)
            {
                for (var _y = _Origin.Y - 1; _y <= _Origin.Y; _y++)
                {
                    for (var _x = _Origin.X - 1; _x <= _Origin.X; _x++)
                    {
                        var _currAngle = Angle(_z, _y, _x, _ScaleOrigin, _CenterLine);
                        if (_currAngle < _minAngle)
                        {
                            _minAngle = _currAngle;
                        }
                    }
                }
            }

            if (_minAngle > 45)
            {
                // if none within 45 degrees, back up along the vector 2.5 units and use that as the angular vector origin
                Vector3D _norm = _LocMode.GetPoint3D(_Target) - _LocMode.GetPoint3D(_Origin);
                _norm.Normalize();
                _AltOrigin = _ScaleOrigin - Vector3D.Multiply(_norm, 2.5);
                _UseAltOrigin = true;
                _Tolerance = 45.5d;
            }
            else
            {
                _UseAltOrigin = false;
                _Tolerance = 45.0001d;
            }
        }
        #endregion

        #region Data
        private ICellLocation _Target;
        private Vector3D _CenterLine;
        private double _Tolerance = 45.0001;
        private Point3D _AltOrigin = new Point3D();
        private bool _UseAltOrigin = false;
        #endregion

        public ICellLocation Target => _Target;

        // TODO: Cone cubic extents in 3D
        // TODO: fix conic capture...doesn't reliably get cells at origin (fall-off angle by distance?)

        private double Angle(int z, int y, int x, Point3D pt, Vector3D target)
            => Vector3D.AngleBetween((new Point3D(x * 5d + 2.5d, y * 5d + 2.5d, z * 5d + 2.5d)) - pt, target);

        public override bool ContainsCell(ICellLocation location)
        {
            // not even in the ballpark
            if (!CellInCube(location))
            {
                return false;
            }

            // ensure mid point of cell is within angular deviation from center line
            var _midPoint = new Point3D(location.X * 5d + 2.5d, location.Y * 5d + 2.5d, location.Z * 5d + 2.5d);
            Vector3D _midVector;
            if (!_UseAltOrigin)
            {
                _midVector = _midPoint - _ScaleOrigin;
            }
            else
            {
                _midVector = _midPoint - _AltOrigin;
            }

            if (Vector3D.AngleBetween(_midVector, _CenterLine) <= _Tolerance)
            {
                // good enough, now is the cell in the geometry
                return base.ContainsCell(location);
            }
            return false;
        }

        public override IGeometricRegion Move(ICellLocation offset)
            => new Cone(_Origin.Add(offset), _LocMode, _Radius, _Target.Add(offset));
    }

    /// <summary>IGeometryBuilder that builds spheres based on the supplied radius</summary>
    [Serializable]
    public class ConeBuilder : IGeometryBuilder
    {
        /// <summary>IGeometryBuilder that builds spheres based on the supplied radius</summary>
        public ConeBuilder(int radius, AnchorFace[] crossingFaces)
        {
            _Radius = radius;
            _CrossingFaces = crossingFaces;
        }

        #region construction
        private int _Radius;
        private AnchorFace[] _CrossingFaces;
        #endregion

        public AnchorFace[] CrossingFaces
        {
            get => _CrossingFaces;
            set => _CrossingFaces = value;
        }

        public int Radius
        {
            get => _Radius;
            set => _Radius = value;
        }

        // IGeometryBuilder Members
        public IGeometricRegion BuildGeometry(LocationAimMode locMode, ICellLocation location)
            => new Cone(location, locMode, _Radius, location.Add(CrossingFaces.GetAnchorOffset()));
    }
}
