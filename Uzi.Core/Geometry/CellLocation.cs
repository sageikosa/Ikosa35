using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Core
{
    [Serializable]
    public class CellLocation : ICellLocation, ITacticalContext, IEnumerable<Intersection>,
        IGeometryAnchorSupplier, IGeometricSize, IGeometricRegion
    {
        #region construction
        public CellLocation(int z, int y, int x)
        {
            _Z = z;
            _Y = y;
            _X = x;
        }

        public CellLocation(ICellLocation cellLoc)
        {
            _Z = cellLoc.Z;
            _Y = cellLoc.Y;
            _X = cellLoc.X;
        }
        #endregion

        #region public static CellLocation GetAdjacentCellLocation(ICellLocation location, params AnchorFace[] adjacentTo)
        public static CellLocation GetAdjacentCellLocation(ICellLocation location, params AnchorFace[] adjacentTo)
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

            return new CellLocation(_z, _y, _x);
        }
        #endregion

        #region private data
        private int _Z;
        private int _Y;
        private int _X;
        #endregion

        public int Z => _Z;
        public int Y => _Y;
        public int X => _X;
        public int LowerZ => _Z;
        public int LowerY => _Y;
        public int LowerX => _X;
        public CellPosition ToCellPosition() => new CellPosition(this);

        public long GetAxialLength(Axis axis) => 1;

        /// <summary>Create a new cell location using the provided location as an offset</summary>
        public CellLocation Add(ICellLocation offset)
            => Add(offset.Z, offset.Y, offset.X);

        /// <summary>Create a new cell location using the offset values</summary>
        public CellLocation Add(int z, int y, int x)
            => new CellLocation(Z + z, Y + y, X + x);

        public Cubic GetCellCube(CellLocation cell2)
            => new Cubic(Math.Min(Z, cell2.Z), Math.Min(Y, cell2.Y), Math.Min(X, cell2.X),
                Math.Max(Z, cell2.Z), Math.Max(Y, cell2.Y), Math.Max(X, cell2.X));

        public override bool Equals(object obj)
        {
            if (obj is ICellLocation _cRef)
            {
                return (Z == _cRef.Z) && (Y == _cRef.Y) && (X == _cRef.X);
            }

            return false;
        }

        #region public override int GetHashCode()
        public override int GetHashCode()
        {
            try
            {
                return (Y * 100 + X) ^ Z;
            }
            catch
            {
                return (Y ^ X) ^ Z;
            }
        }
        #endregion

        public bool Contains(Point3D point)
        {
            if (((_Z * 5d) <= point.Z) && (point.Z <= ((_Z + 1) * 5d)))
            {
                if (((_Y * 5d) <= point.Y) && (point.Y <= ((_Y + 1) * 5d)))
                {
                    return ((_X * 5d) <= point.X) && (point.X <= ((_X + 1) * 5d));
                }
            }
            return false;
        }

        /// <summary>Cell elevation at the gravity face (negatives for high gravity faces)</summary>
        public double CellElevation(AnchorFace baseFace)
        {
            switch (baseFace)
            {
                case AnchorFace.ZLow: return (Z * 5);
                case AnchorFace.ZHigh: return (Z * -5) + 1;
                case AnchorFace.YLow: return (Y * 5);
                case AnchorFace.YHigh: return (Y * -5) + 1;
                case AnchorFace.XLow: return (X * 5);
                case AnchorFace.XHigh:
                default: return (X * -5) + 1;
            }
        }

        #region IEnumerable<Intersection> Members
        public IEnumerator<Intersection> GetEnumerator()
        {
            for (var _z = 0; _z <= 1; _z++)
            {
                for (var _y = 0; _y <= 1; _y++)
                {
                    for (var _x = 0; _x <= 1; _x++)
                    {
                        yield return new Intersection(_Z + _z, _Y + _y, _X + _x);
                    }
                }
            }

            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (var _z = 0; _z <= 1; _z++)
            {
                for (var _y = 0; _y <= 1; _y++)
                {
                    for (var _x = 0; _x <= 1; _x++)
                    {
                        yield return new Intersection(_Z + _z, _Y + _y, _X + _x);
                    }
                }
            }

            yield break;
        }
        #endregion

        #region IControlChange<ICellLocation> Members

        public void AddChangeMonitor(IMonitorChange<ICellLocation> monitor)
        {
        }

        public void RemoveChangeMonitor(IMonitorChange<ICellLocation> monitor)
        {
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        // IGeometryAnchorSupplier Members
        public ICellLocation Location
            => this;

        public LocationAimMode LocationAimMode
            => LocationAimMode.Cell;

        // IGeometricSize Members
        public long ZHeight => 1;
        public long YLength => 1;
        public long XLength => 1;
        public double ZExtent => 1d;
        public double YExtent => 1d;
        public double XExtent => 1d;

        #region IGeometricRegion Members

        public int UpperZ => Z;
        public int UpperY => Y;
        public int UpperX => X;

        public bool ContainsCell(ICellLocation location)
            => ContainsCell(location.Z, location.Y, location.X);

        public bool ContainsCell(int z, int y, int x)
            => (z == Z) && (y == Y) && (x == X);

        public IEnumerable<ICellLocation> AllCellLocations()
        {
            yield return this;
            yield break;
        }

        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = (Z * 5) + 2.5,
                Y = (Y * 5) + 2.5,
                X = (X * 5) + 2.5
            };

        public IGeometricRegion Move(ICellLocation offset)
            => new CellLocation(Z + offset.Z, Y + offset.Y, X + offset.X);

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
            => ContainsCell(location);

        #endregion
    }
}