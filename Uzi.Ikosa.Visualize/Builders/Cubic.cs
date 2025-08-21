using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    public class Cubic : IGeometricRegion, ICellLocation, IGeometricSize, IEquatable<Cubic>
    {
        #region ctor(...)
        public Cubic(Cubic cubic) :
            this(cubic, cubic)
        {
        }

        public Cubic(int z, int y, int x, int topz, int topy, int topx)
        {
            _Z = z;
            _Y = y;
            _X = x;
            _ZExtent = topz - z + 1;
            _YExtent = topy - y + 1;
            _XExtent = topx - x + 1;
            CalcAll();
        }

        public Cubic(ICellLocation location, long zHeight, long yLength, long xLength)
        {
            _Z = location.Z;
            _Y = location.Y;
            _X = location.X;
            _ZExtent = zHeight;
            _YExtent = yLength;
            _XExtent = xLength;
            CalcAll();
        }

        public Cubic(ICellLocation location, IGeometricSize size)
        {
            _Z = location.Z;
            _Y = location.Y;
            _X = location.X;
            _ZExtent = size.ZExtent;
            _YExtent = size.YExtent;
            _XExtent = size.XExtent;
            CalcAll();
        }

        public Cubic(ICellLocation location, double zHeight, double yLength, double xLength)
        {
            _Z = location.Z;
            _Y = location.Y;
            _X = location.X;
            _ZExtent = zHeight;
            _YExtent = yLength;
            _XExtent = xLength;
            CalcAll();
        }

        protected Cubic(SerializationInfo info, StreamingContext context)
            : this(info, context, false)
        {
        }

        protected Cubic(SerializationInfo info, StreamingContext context, bool inherited)
        {
            var _prefix = !inherited ? string.Empty : $@"{nameof(Cubic)}+";
            string _fullName(string field)
                => $@"{_prefix}{field}";

            _Z = info.GetInt32(_fullName(nameof(_Z)));
            _Y = info.GetInt32(_fullName(nameof(_Y)));
            _X = info.GetInt32(_fullName(nameof(_X)));
            _HiZ = info.GetInt32(_fullName(nameof(_HiZ)));
            _HiY = info.GetInt32(_fullName(nameof(_HiY)));
            _HiX = info.GetInt32(_fullName(nameof(_HiX)));
            _ZExt = info.GetInt64(_fullName(nameof(_ZExt)));
            _YExt = info.GetInt64(_fullName(nameof(_YExt)));
            _XExt = info.GetInt64(_fullName(nameof(_XExt)));

            //double _fetch(string key, long defaultVal)
            //{
            //    try { return info.GetDouble(_fullName(key)); }
            //    catch { return defaultVal; }
            //}
            _ZExtent = info.GetDouble(_fullName(nameof(_ZExtent)));
            _YExtent = info.GetDouble(_fullName(nameof(_YExtent)));
            _XExtent = info.GetDouble(_fullName(nameof(_XExtent)));
        }
        #endregion

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var _prefix = GetType() == typeof(Cubic) ? string.Empty : $@"{nameof(Cubic)}+";
            string _fullName(string field)
                => $@"{_prefix}{field}";

            info.AddValue(_fullName(nameof(_Z)), _Z);
            info.AddValue(_fullName(nameof(_Y)), _Y);
            info.AddValue(_fullName(nameof(_X)), _X);
            info.AddValue(_fullName(nameof(_ZExtent)), _ZExtent);
            info.AddValue(_fullName(nameof(_YExtent)), _YExtent);
            info.AddValue(_fullName(nameof(_XExtent)), _XExtent);
            info.AddValue(_fullName(nameof(_HiZ)), _HiZ);
            info.AddValue(_fullName(nameof(_HiY)), _HiY);
            info.AddValue(_fullName(nameof(_HiX)), _HiX);
            info.AddValue(_fullName(nameof(_ZExt)), _ZExt);
            info.AddValue(_fullName(nameof(_YExt)), _YExt);
            info.AddValue(_fullName(nameof(_XExt)), _XExt);
        }

        protected virtual void CalcAll()
        {
            _ZExt = Math.Max(Convert.ToInt64(_ZExtent), 1);
            _YExt = Math.Max(Convert.ToInt64(_YExtent), 1);
            _XExt = Math.Max(Convert.ToInt64(_XExtent), 1);
            _HiZ = Convert.ToInt32(_Z + _ZExt - 1);
            _HiY = Convert.ToInt32(_Y + _YExt - 1);
            _HiX = Convert.ToInt32(_X + _XExt - 1);
        }

        #region data
        protected int _Z;
        protected int _Y;
        protected int _X;
        private double _ZExtent;
        private double _YExtent;
        private double _XExtent;
        protected int _HiZ;
        protected int _HiY;
        protected int _HiX;
        protected long _ZExt;
        protected long _YExt;
        protected long _XExt;
        #endregion

        public int Z { get => _Z; protected set { _Z = value; _HiZ = Convert.ToInt32(_Z + _ZExt - 1); } }
        public int Y { get => _Y; protected set { _Y = value; _HiY = Convert.ToInt32(_Y + _YExt - 1); } }
        public int X { get => _X; protected set { _X = value; _HiX = Convert.ToInt32(_X + _XExt - 1); } }
        public int LowerZ => _Z;
        public int LowerY => _Y;
        public int LowerX => _X;

        public CellPosition ToCellPosition() => new CellPosition(this);

        public int UpperZ => _HiZ;
        public int UpperY => _HiY;
        public int UpperX => _HiX;

        public long ZHeight => _ZExt;
        public long YLength => _YExt;
        public long XLength => _XExt;

        public double ZExtent { get => _ZExtent; protected set { _ZExtent = value; _ZExt = Math.Max(Convert.ToInt64(_ZExtent), 1); _HiZ = Convert.ToInt32(_Z + _ZExt - 1); } }
        public double YExtent { get => _YExtent; protected set { _YExtent = value; _YExt = Math.Max(Convert.ToInt64(_YExtent), 1); _HiY = Convert.ToInt32(_Y + _YExt - 1); } }
        public double XExtent { get => _XExtent; protected set { _XExtent = value; _XExt = Math.Max(Convert.ToInt64(_XExtent), 1); _HiX = Convert.ToInt32(_X + _XExt - 1); } }

        public long GetAxialLength(Axis axis) =>
            axis == Axis.Z ? ZHeight :
            axis == Axis.Y ? YLength :
            XLength;

        public long CellCount => _ZExt * _YExt * _XExt;

        public Point3D CenterPoint
            => new Point3D(
                (_X + _XExtent / 2) * 5,
                (_Y + _YExtent / 2) * 5,
                (_Z + _ZExtent / 2) * 5);

        #region public virtual IEnumerable<List<ICellLocation>> AllCubicPartitions()
        /// <summary>Partitions a cube into lists of cells for parallelizing operations</summary>
        public virtual IEnumerable<List<CellPosition>> AllCubicPartitions()
        {
            if ((_XExt >= _YExt) && (_XExt >= _ZExt))
            {
                // X has longest strips
                for (var _z = _Z; _z <= _HiZ; _z++)
                {
                    for (var _y = _Y; _y <= _HiY; _y++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _x = _X; _x <= _HiX; _x++)
                        {
                            var _cLoc = new CellPosition(_z, _y, _x);
                            if (ContainsCell(_cLoc))
                            {
                                _list.Add(_cLoc);
                            }
                        }
                        yield return _list;
                    }
                }
            }
            else if (_YExt >= _ZExt)
            {
                // Y has longest strips
                for (var _z = _Z; _z <= _HiZ; _z++)
                {
                    for (var _x = _X; _x <= _HiX; _x++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _y = _Y; _y <= _HiY; _y++)
                        {
                            var _cLoc = new CellPosition(_z, _y, _x);
                            if (ContainsCell(_cLoc))
                            {
                                _list.Add(_cLoc);
                            }
                        }
                        yield return _list;
                    }
                }
            }
            else
            {
                // Z has longest strips
                for (var _y = _Y; _y <= _HiY; _y++)
                {
                    for (var _x = _X; _x <= _HiX; _x++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _z = _Z; _z <= _HiZ; _z++)
                        {
                            var _cLoc = new CellPosition(_z, _y, _x);
                            if (ContainsCell(_cLoc))
                            {
                                _list.Add(_cLoc);
                            }
                        }
                        yield return _list;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public virtual IEnumerable<ICellLocation> AllCellLocations()
        public virtual IEnumerable<ICellLocation> AllCellLocations()
        {
            for (var _z = _Z; _z <= _HiZ; _z++)
            {
                for (var _y = _Y; _y <= _HiY; _y++)
                {
                    for (var _x = _X; _x <= _HiX; _x++)
                    {
                        var _cLoc = new CellPosition(_z, _y, _x);
                        if (ContainsCell(_cLoc))
                        {
                            yield return _cLoc;
                        }
                    }
                }
            }

            yield break;
        }
        #endregion

        #region public virtual IEnumerable<List<CellPosition>> AllIntersectionPartitions()
        /// <summary>Partitions a cube into lists of cells for parallelizing operations</summary>
        public virtual IEnumerable<List<CellPosition>> AllIntersectionPartitions()
        {
            if ((_XExt >= _YExt) && (_XExt >= _ZExt))
            {
                // X has longest strips
                for (var _z = _Z; _z <= _HiZ + 1; _z++)
                {
                    for (var _y = _Y; _y <= _HiY + 1; _y++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _x = _X; _x <= _HiX + 1; _x++)
                        {
                            _list.Add(new CellPosition(_z, _y, _x));
                        }
                        yield return _list;
                    }
                }
            }
            else if (_YExt >= _ZExt)
            {
                // Y has longest strips
                for (var _z = _Z; _z <= _HiZ + 1; _z++)
                {
                    for (var _x = _X; _x <= _HiX + 1; _x++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _y = _Y; _y <= _HiY + 1; _y++)
                        {
                            _list.Add(new CellPosition(_z, _y, _x));
                        }
                        yield return _list;
                    }
                }
            }
            else
            {
                // Z has longest strips
                for (var _y = _Y; _y <= _HiY + 1; _y++)
                {
                    for (var _x = _X; _x <= _HiX + 1; _x++)
                    {
                        var _list = new List<CellPosition>();
                        for (var _z = _Z; _z <= _HiZ + 1; _z++)
                        {
                            _list.Add(new CellPosition(_z, _y, _x));
                        }
                        yield return _list;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public virtual IEnumerable<CellPosition> AllIntersections()
        public virtual IEnumerable<CellPosition> AllIntersections()
        {
            for (var _z = _Z; _z <= _HiZ + 1; _z++)
            {
                for (var _y = _Y; _y <= _HiY + 1; _y++)
                {
                    for (var _x = _X; _x <= _HiX + 1; _x++)
                    {
                        yield return new CellPosition(_z, _y, _x);
                    }
                }
            }

            yield break;
        }
        #endregion

        public bool ContainsCell(ICellLocation location)
            => ContainsCell(location.Z, location.Y, location.X);

        public bool ContainsCell(int z, int y, int x)
            => (z >= _Z) && (z <= _HiZ)
            && (y >= _Y) && (y <= _HiY)
            && (x >= _X) && (x <= _HiX);

        /// <summary>Indicates whether a target cubic overlaps this cubic in some way</summary>
        public bool IsOverlapping(IGeometricRegion target)
            => (target != null)
            && (_Z <= target.UpperZ) && (target.LowerZ <= _HiZ)
            && (_Y <= target.UpperY) && (target.LowerY <= _HiY)
            && (_X <= target.UpperX) && (target.LowerX <= _HiX);

        /// <summary>Gets a cube of the cells that face outwards towards the specified face</summary>
        public Cubic EdgeCubic(AnchorFace facingFace)
        {
            #region inner and outer edges
            switch (facingFace)
            {
                case AnchorFace.ZHigh:
                    return new Cubic(_HiZ, _Y, _X, _HiZ, _HiY, _HiX);
                case AnchorFace.ZLow:
                    return new Cubic(_Z, _Y, _X, _Z, _HiY, _HiX);
                case AnchorFace.YHigh:
                    return new Cubic(_Z, _HiY, _X, _HiZ, _HiY, _HiX);
                case AnchorFace.YLow:
                    return new Cubic(_Z, _Y, _X, _HiZ, _Y, _HiX);
                case AnchorFace.XHigh:
                    return new Cubic(_Z, _Y, _HiX, _HiZ, _HiY, _HiX);
                default: //case Visualize.AnchorFace.XLow:
                    return new Cubic(_Z, _Y, _X, _HiZ, _HiY, _X);
            }
            #endregion
        }

        #region public Cubic ReducedCubic(AnchorFace halfFace, int reduction)
        public Cubic ReducedCubic(AnchorFace halfFace, int reduction)
        {
            switch (halfFace)
            {
                case AnchorFace.ZHigh:
                    return new Cubic(_Z + reduction, _Y, _X, _HiZ, _HiY, _HiX);
                case AnchorFace.ZLow:
                    return new Cubic(this, _ZExt - reduction, _YExt, _XExt);
                case AnchorFace.YHigh:
                    return new Cubic(_Z, _Y + reduction, _X, _HiZ, _HiY, _HiX);
                case AnchorFace.YLow:
                    return new Cubic(this, _ZExt, _YExt - reduction, _XExt);
                case AnchorFace.XHigh:
                    return new Cubic(_Z, _Y, _X + reduction, _HiZ, _HiY, _HiX);
                default:
                    return new Cubic(this, _ZExt, _YExt, _XExt - reduction);
            }
        }
        #endregion

        #region public Cubic OffsetCubic(AnchorFace[] offsetFaces)
        public Cubic OffsetCubic(params AnchorFace[] offsetFaces)
        {
            var _z = _Z + (offsetFaces.Contains(AnchorFace.ZHigh) ? 1 :
                offsetFaces.Contains(AnchorFace.ZLow) ? -1 : 0);
            var _y = _Y + (offsetFaces.Contains(AnchorFace.YHigh) ? 1 :
                offsetFaces.Contains(AnchorFace.YLow) ? -1 : 0);
            var _x = _X + (offsetFaces.Contains(AnchorFace.XHigh) ? 1 :
                offsetFaces.Contains(AnchorFace.XLow) ? -1 : 0);
            return new Cubic(new CellPosition(_z, _y, _x), _ZExtent, _YExtent, _XExtent);
        }
        #endregion

        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = (_Z * 5) + (_HiZ + 1 - _Z) * 2.5,
                Y = (_Y * 5) + (_HiY + 1 - _Y) * 2.5,
                X = (_X * 5) + (_HiX + 1 - _X) * 2.5
            };

        public IGeometricRegion Move(ICellLocation offset)
            => new Cubic((new CellPosition(this).Move(offset)) as ICellLocation, this);

        #region public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
        {
            switch (surfaceFace)
            {
                case AnchorFace.ZLow: return location.Z == _Z;
                case AnchorFace.YLow: return location.Y == _Y;
                case AnchorFace.XLow: return location.X == _X;
                case AnchorFace.ZHigh: return location.Z == _HiZ;
                case AnchorFace.YHigh: return location.Y == _HiY;
                case AnchorFace.XHigh: default: return location.X == _HiX;
            }
        }
        #endregion

        public bool Equals(Cubic other)
            => (_Z == other.Z)
            && (_Y == other.Y)
            && (_X == other.X)
            && (_HiZ == other.UpperZ)
            && (_HiY == other.UpperY)
            && (_HiX == other.UpperX);

        public override string ToString()
            => $@"({_Z},{_Y},{_X});({_HiZ},{_HiY},{_HiX})";
    }

    [Serializable]
    public class CubicBuilder : IGeometryBuilder
    {
        #region state
        private readonly IGeometricSize _Size;
        private readonly ICellLocation _Offset;
        #endregion

        #region construction
        /// <summary>
        /// defines a CubicBuilder for the specified size, so that the relative (0,0,0) cell is at the requested location
        /// </summary>
        public CubicBuilder(IGeometricSize size)
        {
            _Size = size;
            _Offset = null;
        }

        /// <summary>
        /// defines a CubicBuilder for the specified size, and applies a compensating offset so that the zeroCell is at (0,0,0) in relative space
        /// </summary>
        public CubicBuilder(IGeometricSize size, ICellLocation zeroCell)
        {
            _Size = size;
            _Offset = zeroCell;
        }
        #endregion

        public IGeometricSize Size => _Size;
        public ICellLocation ZeroCell => _Offset;

        public IGeometricRegion BuildGeometry(LocationAimMode locMode, ICellLocation location)
            => (_Offset == null)
            ? new Cubic(location, _Size)
            : new Cubic(new CellPosition(
                location.Z - _Offset.Z,
                location.Y - _Offset.Y,
                location.X - _Offset.X),
                _Size);
    }
}
