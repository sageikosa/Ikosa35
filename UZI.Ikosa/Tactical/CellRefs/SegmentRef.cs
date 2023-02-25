using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public readonly struct SegmentRef : ICellLocation, ITacticalContext
    {
        public SegmentRef(int z, int y, int x, double distance, CellStructure cellSpace, Point3D entryPoint, Point3D exitPoint, Room room)
        {
            _Z = z;
            _Y = y;
            _X = x;
            _CellSpace = cellSpace;
            _Room = room;
            _Distance = distance;
            _EntryPoint = entryPoint;
            _ExitPoint = exitPoint;
            _IsPoint = _EntryPoint == _ExitPoint;
            _Actual = true;
        }

        #region private data
        private readonly int _Z;
        private readonly int _Y;
        private readonly int _X;

        private readonly CellStructure _CellSpace;
        private readonly Room _Room;

        private readonly double _Distance;

        private readonly Point3D _ExitPoint;
        private readonly Point3D _EntryPoint;
        private readonly bool _IsPoint;
        private readonly bool _Actual;
        #endregion

        public int Z => _Z;
        public int Y => _Y;
        public int X => _X;

        public CellStructure CellSpace => _CellSpace;
        public Room Room => _Room;

        public double Distance => _Distance;

        public Point3D EntryPoint => _EntryPoint;
        public Point3D ExitPoint => _ExitPoint;
        public bool IsPoint => _IsPoint;

        /// <summary>
        /// false if constructed via default(SegmentRef)
        /// </summary>
        public bool IsActual => _Actual;

        // cellLocation
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

        public bool ContainsCell(ICellLocation location)
            => ContainsCell(location.Z, location.Y, location.X);

        public bool ContainsCell(int z, int y, int x)
            => (z == Z) && (y == Y) && (x == X);

        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = (Z * 5) + 2.5,
                Y = (Y * 5) + 2.5,
                X = (X * 5) + 2.5
            };

        public CellPosition ToCellPosition() => new CellPosition(this);

        // linear cell ref

        #region private Intersection NearestIntersection(Point3D pt)
        private Intersection NearestIntersection(Point3D pt)
        {
            var _nz = Z;
            var _ny = Y;
            var _nx = X;
            var _current = this.Point3D();
            if (pt.Z > _current.Z + 2.5)
                _nz++;
            if (pt.Y > _current.Y + 2.5)
                _ny++;
            if (pt.X > _current.X + 2.5)
                _nx++;
            return new Intersection(_nz, _ny, _nx);
        }
        #endregion

        public Intersection NearestEntrance
            => NearestIntersection(EntryPoint);

        public Intersection NearestExit
            => NearestIntersection(ExitPoint);

        #region public AnchorFaceList EntryFaces { get; }
        public AnchorFaceList EntryFaces
        {
            get
            {
                var _faces = AnchorFaceList.None;
                if (EntryPoint.X.CloseEnough(X * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.XLow);
                if (EntryPoint.Y.CloseEnough(Y * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.YLow);
                if (EntryPoint.Z.CloseEnough(Z * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.ZLow);
                if (EntryPoint.X.CloseEnough((X + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.XHigh);
                if (EntryPoint.Y.CloseEnough((Y + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.YHigh);
                if (EntryPoint.Z.CloseEnough((Z + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.ZHigh);
                return _faces;
            }
        }
        #endregion

        #region public AnchorFaceList ExitFaces { get; }
        public AnchorFaceList ExitFaces
        {
            get
            {
                var _faces = AnchorFaceList.None;
                if (ExitPoint.X.CloseEnough(X * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.XLow);
                if (ExitPoint.Y.CloseEnough(Y * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.YLow);
                if (ExitPoint.Z.CloseEnough(Z * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.ZLow);
                if (ExitPoint.X.CloseEnough((X + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.XHigh);
                if (ExitPoint.Y.CloseEnough((Y + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.YHigh);
                if (ExitPoint.Z.CloseEnough((Z + 1) * 5d, 0.01d)) _faces = _faces.Add(AnchorFace.ZHigh);
                return _faces;
            }
        }
        #endregion

        public bool BlocksDetect
        {
            get
            {
                // NOTE: cellspace has already determined locator (and hopefully geometric) effects
                return CellSpace.BlocksDetect(Z, Y, X, _EntryPoint, _ExitPoint);
            }
        }

        public bool BlocksPath
        {
            get
            {
                // NOTE: cellspace has already determined locator (and hopefully geometric) effects
                return CellSpace.BlocksPath(Z, Y, X, _EntryPoint, _ExitPoint);
            }
        }

    }
}
