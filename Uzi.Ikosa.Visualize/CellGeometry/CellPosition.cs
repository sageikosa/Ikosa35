using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    public struct CellPosition : ICellLocation, IEquatable<ICellLocation>, IGeometricRegion
    {
        #region construction
        public CellPosition(int z, int y, int x)
        {
            _Z = z;
            _Y = y;
            _X = x;
        }

        public CellPosition(ICellLocation cellLoc)
        {
            _Z = cellLoc.Z;
            _Y = cellLoc.Y;
            _X = cellLoc.X;
        }
        #endregion

        #region state
        private int _Z;
        private int _Y;
        private int _X;
        #endregion

        public int Z { get { return _Z; } set { _Z = value; } }
        public int Y { get { return _Y; } set { _Y = value; } }
        public int X { get { return _X; } set { _X = value; } }

        public CellPosition ToCellPosition() => this;

        public static CellPosition GetAdjacentCellPosition(ICellLocation location, params AnchorFace[] adjacentTo)
        {
            var _z = location.Z;
            var _y = location.Y;
            var _x = location.X;
            if (adjacentTo.Contains(AnchorFace.ZHigh))
            {
                _z++;
            }
            else if (adjacentTo.Contains(AnchorFace.ZLow))
            {
                _z--;
            }

            if (adjacentTo.Contains(AnchorFace.YHigh))
            {
                _y++;
            }
            else if (adjacentTo.Contains(AnchorFace.YLow))
            {
                _y--;
            }

            if (adjacentTo.Contains(AnchorFace.XHigh))
            {
                _x++;
            }
            else if (adjacentTo.Contains(AnchorFace.XLow))
            {
                _x--;
            }

            return new CellPosition(_z, _y, _x);
        }

        #region IEquatable<ICellLocation> Members

        public bool Equals(ICellLocation other)
            => (_Z == other.Z) && (_Y == other.Y) && (_X == other.X);

        #endregion

        #region IGeometricRegion Members

        public int LowerZ => _Z;
        public int LowerY => _Y;
        public int LowerX => _X;
        public int UpperZ => _Z;
        public int UpperY => _Y;
        public int UpperX => _X;

        /// <summary>Point at a cell centroid</summary>
        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = _Z * 5d + 2.5d,
                Y = _Y * 5d + 2.5d,
                X = _X * 5d + 2.5d
            };

        public IEnumerable<ICellLocation> AllCellLocations()
        {
            yield return this;
            yield break;
        }

        public bool ContainsCell(ICellLocation location)
            => ContainsCell(location.Z, location.Y, location.X);

        public bool ContainsCell(int z, int y, int x)
            => (_Z == z) && (_Y == y) && (_X == x);

        public IGeometricRegion Move(ICellLocation offset)
            => new CellPosition(Z = _Z + offset.Z, Y = _Y + offset.Y, X = _X + offset.X);

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
            => (surfaceFace == AnchorFace.ZLow) ? location.Z == _Z
            : (surfaceFace == AnchorFace.YLow) ? location.Y == _Y
            : (surfaceFace == AnchorFace.XLow) ? location.X == _X
            : (surfaceFace == AnchorFace.ZHigh) ? location.Z == _Z
            : (surfaceFace == AnchorFace.YHigh) ? location.Y == _Y
            : (surfaceFace == AnchorFace.XHigh) ? location.X == _X
            : false;

        #endregion
    }
}