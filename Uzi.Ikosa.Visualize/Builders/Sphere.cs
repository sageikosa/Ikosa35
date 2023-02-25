using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    public class Sphere : IGeometricRegion
    {
        #region Construction
        public Sphere(ICellLocation origin, LocationAimMode locMode, int radiusInCells)
        {
            _Origin = origin;
            _LocMode = locMode;
            _Radius = Math.Abs(radiusInCells);

            _ScaleOrigin = _LocMode.GetPoint3D(_Origin);
            _ScaleRadius = _Radius * 5.0d;
            _SquaredRadius = _ScaleRadius * _ScaleRadius;
        }
        #endregion

        #region data
        protected ICellLocation _Origin;
        protected LocationAimMode _LocMode;
        protected int _Radius;

        protected Point3D _ScaleOrigin;
        protected double _ScaleRadius;
        protected double _SquaredRadius;
        #endregion

        public long ZHeight => _Radius * 2;
        public long YLength => _Radius * 2;
        public long XLength => _Radius * 2;
        public virtual int LowerZ => _Origin.Z - _Radius;
        public int LowerY => _Origin.Y - _Radius;
        public int LowerX => _Origin.X - _Radius;
        public virtual int UpperZ => _Origin.Z + _Radius - 1;
        public int UpperY => _Origin.Y + _Radius - 1;
        public int UpperX => _Origin.X + _Radius - 1;
        public double ZExtent => ZHeight;
        public double YExtent => YLength;
        public double XExtent => XLength;

        public bool CellInCube(ICellLocation location)
            => (location.Z >= LowerZ) && (location.Y >= LowerY) && (location.X >= LowerX)
                && (location.Z <= UpperZ) && (location.Y <= UpperY) && (location.X <= UpperX);

        protected virtual bool IsIntersectInGeometry(int z, int y, int x)
            => ((new Point3D(x * 5d, y * 5d, z * 5d)) - _ScaleOrigin).LengthSquared <= _SquaredRadius;

        public bool ContainsCell(int z, int y, int x)
            => ContainsCell(new CellPosition(z, y, x));

        #region public bool ContainsCell(ICellLocation location)
        /// <summary>
        /// Determines inclusion in sphere if each face is in at least one of its vertices.
        /// </summary>
        /// <returns>true if the cell is in</returns>
        public virtual bool ContainsCell(ICellLocation location)
        {
            // not even in the ballpark

            if (!CellInCube(location))
                return false;

            var _lz = false;
            var _ly = false;
            var _lx = false;
            var _uz = false;
            var _uy = false;
            var _ux = false;
            {
                var _zMyMxM = IsIntersectInGeometry(location.Z, location.Y, location.X);
                var _zPyPxP = IsIntersectInGeometry(location.Z + 1, location.Y + 1, location.X + 1);

                // any pair of opposite points "in" indicates success
                if (_zMyMxM && _zPyPxP)
                    return true;
                else if (_zMyMxM)
                {
                    // if a single point is in, then set its three face flags to true
                    _lz = _ly = _lx = true;
                }
                else if (_zPyPxP)
                {
                    // if a single point is in, then set its three face flags to true
                    _uz = _uy = _ux = true;
                }
            }

            {
                var _zPyMxM = IsIntersectInGeometry(location.Z + 1, location.Y, location.X);
                var _zMyPxP = IsIntersectInGeometry(location.Z, location.Y + 1, location.X + 1);

                // any pair of opposite points "in" indicates success
                if (_zPyMxM && _zMyPxP)
                    return true;
                else if (_zPyMxM)
                {
                    // if a single point is in, then set its three face flags to true
                    _uz = _ly = _lx = true;
                }
                else if (_zMyPxP)
                {
                    // if a single point is in, then set its three face flags to true
                    _lz = _uy = _ux = true;
                }

                // if all 6 face flags are true indicates success
                if (_lz && _ly && _lx & _uz & _uy && _ux)
                    return true;
            }

            {
                var _zPyPxM = IsIntersectInGeometry(location.Z + 1, location.Y + 1, location.X);
                var _zMyMxP = IsIntersectInGeometry(location.Z, location.Y, location.X + 1);

                // any pair of opposite points "in" indicates success
                if (_zPyPxM && _zMyMxP)
                    return true;
                else if (_zPyPxM)
                {
                    // if a single point is in, then set its three face flags to true
                    _uz = _uy = _lx = true;
                }
                else if (_zMyMxP)
                {
                    // if a single point is in, then set its three face flags to true
                    _lz = _ly = _ux = true;
                }

                // if all 6 face flags are true indicates success
                if (_lz && _ly && _lx & _uz & _uy && _ux)
                    return true;
            }

            {
                var _zMyPxM = IsIntersectInGeometry(location.Z, location.Y + 1, location.X);
                var _zPyMxP = IsIntersectInGeometry(location.Z + 1, location.Y, location.X + 1);

                // any pair of opposite points "in" indicates success
                if (_zMyPxM && _zPyMxP)
                    return true;
                else if (_zMyPxM)
                {
                    // if a single point is in, then set its three face flags to true
                    _lz = _uy = _lx = true;
                }
                else if (_zPyMxP)
                {
                    // if a single point is in, then set its three face flags to true
                    _uz = _ly = _ux = true;
                }

                // if all 6 face flags are true indicates success
                if (_lz && _ly && _lx & _uz & _uy && _ux)
                    return true;
            }

            return false;
        }
        #endregion

        #region public virtual IEnumerable<ICellLocation> AllCellLocations()
        /// <summary>Enumerates all cell locations that are in the specific geometry</summary>
        public virtual IEnumerable<ICellLocation> AllCellLocations()
        {
            for (var _z = LowerZ; _z <= UpperZ; _z++)
                for (var _y = LowerY; _y <= UpperY; _y++)
                    for (var _x = LowerX; _x < UpperX; _x++)
                    {
                        var _cLoc = new CellPosition(_z, _y, _x);
                        if (ContainsCell(_cLoc))
                            yield return _cLoc;
                    }
            yield break;
        }
        #endregion

        public Point3D GetPoint3D() { return _ScaleOrigin; }

        public virtual IGeometricRegion Move(ICellLocation offset)
            => new Sphere(_Origin.Add(offset), _LocMode, _Radius);

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
            => this.IsCellUnboundAtFace(location, surfaceFace);
    }

    /// <summary>IGeometryBuilder that builds spheres based on the supplied radius</summary>
    [Serializable]
    public class SphereBuilder : IGeometryBuilder
    {
        /// <summary>IGeometryBuilder that builds spheres based on the supplied radius</summary>
        public SphereBuilder(int radius)
        {
            _Radius = radius;
        }

        #region state
        private int _Radius;
        #endregion

        public int Radius { get => _Radius; set => _Radius = value; }

        // IGeometryBuilder Members
        public IGeometricRegion BuildGeometry(LocationAimMode locMode, ICellLocation location)
            => new Sphere(location, locMode, _Radius);
    }
}
