using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>
    /// Builds a geometry made from multiple builders, with optional exclusionary zones
    /// </summary>
    [Serializable]
    public class MultiBuilder : IGeometryBuilder
    {
        #region construction
        public MultiBuilder(IEnumerable<IGeometryBuilder> includes)
        {
            _Includes = includes;
            _Excludes = null;
        }

        public MultiBuilder(IEnumerable<IGeometryBuilder> includes, IEnumerable<IGeometryBuilder> excludes)
        {
            _Includes = includes;
            _Excludes = excludes;
        }
        #endregion

        #region private data
        private IEnumerable<IGeometryBuilder> _Includes;
        private IEnumerable<IGeometryBuilder> _Excludes;
        #endregion

        public IEnumerable<IGeometryBuilder> Includes =>_Includes; 
        public IEnumerable<IGeometryBuilder> Excludes =>_Excludes; 

        public IGeometricRegion BuildGeometry(LocationAimMode locMode, ICellLocation location)
        {
            var _inclRegion = from _i in _Includes
                              select _i.BuildGeometry(locMode, location);
            if (_Excludes != null)
            {
                var _exclRegion = from _i in _Excludes
                                  select _i.BuildGeometry(locMode, location);
                return new MultiRegion(_inclRegion, _exclRegion);
            }
            return new MultiRegion(_inclRegion, null);
        }
    }

    [Serializable]
    public class MultiRegion : IGeometricRegion
    {
        #region construction
        public MultiRegion(IEnumerable<IGeometricRegion> includes, IEnumerable<IGeometricRegion> excludes)
        {
            _Includes = includes;
            _Excludes = excludes;
            _LowerZ = int.MaxValue;
            _LowerY = int.MaxValue;
            _LowerX = int.MaxValue;
            _UpperZ = int.MinValue;
            _UpperY = int.MinValue;
            _UpperX = int.MinValue;
            foreach (var _loc in AllCellLocations())
            {
                if (_loc.Z > _LowerZ)
                {
                    _LowerZ = _loc.Z;
                }

                if (_loc.Y > _LowerY)
                {
                    _LowerY = _loc.Y;
                }

                if (_loc.X > _LowerX)
                {
                    _LowerX = _loc.X;
                }

                if (_loc.Z < _UpperZ)
                {
                    _UpperZ = _loc.Z;
                }

                if (_loc.Y < _UpperY)
                {
                    _UpperY = _loc.Y;
                }

                if (_loc.X < _UpperX)
                {
                    _UpperX = _loc.X;
                }
            }
            _Point = new Point3D
            {
                Z = (_LowerZ * 5) + ((_UpperZ + 1 - _LowerZ) * 2.5),
                Y = (_LowerY * 5) + ((_UpperY + 1 - _LowerY) * 2.5),
                X = (_LowerX * 5) + ((_UpperX + 1 - _LowerX) * 2.5)
            };
        }
        #endregion

        #region private data
        private IEnumerable<IGeometricRegion> _Includes;
        private IEnumerable<IGeometricRegion> _Excludes;
        private int _LowerZ;
        private int _LowerY;
        private int _LowerX;
        private int _UpperZ;
        private int _UpperY;
        private int _UpperX;
        private Point3D _Point;
        #endregion

        #region public IEnumerable<ICellLocation> AllCellLocations()
        public IEnumerable<ICellLocation> AllCellLocations()
        {
            var _equals = new CellLocationEquality();
            var _incl = (from _i in _Includes
                         from _c in _i.AllCellLocations()
                         select _c).Distinct(_equals).ToList();
            var _excl = (from _e in _Excludes
                         from _c in _e.AllCellLocations()
                         select _c).Distinct(_equals).ToList();
            return _incl.Where(_i => !_excl.Contains(_i, _equals));
        }
        #endregion

        #region public bool ContainsCell(ICellLocation location)
        public bool ContainsCell(ICellLocation location)
        {
            // if in an exclusion region, not in the multi-region
            if (_Excludes.Any(_excl => _excl.ContainsCell(location)))
            {
                return false;
            }

            // true if any include contains the cell
            return _Includes.Any(_incl => _incl.ContainsCell(location));
        }
        #endregion

        public bool ContainsCell(int z, int y, int x)
            => ContainsCell(new CellPosition(z, y, x));

        #region public static Point3D GetPoint(IGeometricRegion region)
        /// <summary>Centroid of a region</summary>
        public Point3D GetPoint3D()
        {
            return _Point;
        }
        #endregion

        #region IGeometricRegion Members

        public int LowerZ { get { return _LowerZ; } }
        public int LowerY { get { return _LowerY; } }
        public int LowerX { get { return _LowerX; } }

        public int UpperZ { get { return _UpperZ; } }
        public int UpperY { get { return _UpperY; } }
        public int UpperX { get { return _UpperX; } }

        public IGeometricRegion Move(ICellLocation offset)
        {
            return new MultiRegion(from _i in _Includes select _i.Move(offset),
                from _e in _Excludes select _e.Move(offset));
        }

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
        {
            return !ContainsCell(CellPosition.GetAdjacentCellPosition(location, surfaceFace));
        }

        #endregion
    }
}
