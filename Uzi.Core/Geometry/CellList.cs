using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Core
{
    [Serializable]
    /// <summary>Geometric region as a list of cells</summary>
    public class CellList : IGeometricRegion
    {
        #region construction
        public CellList(IEnumerable<ICellLocation> cellList, double narrowZ, double narrowY, double narrowX)
        {
            _List = cellList.ToList();
            _Z = _List.Select(_c => _c.Z).Min();
            _Y = _List.Select(_c => _c.Y).Min();
            _X = _List.Select(_c => _c.X).Min();
            _TopZ = _List.Select(_c => _c.Z).Max();
            _TopY = _List.Select(_c => _c.Y).Max();
            _TopX = _List.Select(_c => _c.X).Max();
            _ChokeZ = narrowZ;
            _ChokeY = narrowY;
            _ChokeX = narrowX;
        }
        #endregion

        #region Private Data
        private List<ICellLocation> _List;
        private int _Z;
        private int _Y;
        private int _X;
        private int _TopZ;
        private int _TopY;
        private int _TopX;
        private double _ChokeZ;
        private double _ChokeY;
        private double _ChokeX;
        #endregion

        public int LowerZ => _Z;
        public int LowerY => _Y;
        public int LowerX => _X;
        public int UpperZ => _TopZ;
        public int UpperY => _TopY;
        public int UpperX => _TopX;
        public long ZHeight => _TopZ - (long)_Z + 1;
        public long YLength => _TopY - (long)_Y + 1;
        public long XLength => _TopX - (long)_X + 1;
        public double ZExtent => ZHeight;
        public double YExtent => YLength;
        public double XExtent => XLength;

        public double ZNarrowest => _ChokeZ;
        public double YNarrowest => _ChokeY;
        public double XNarrowest => _ChokeX;

        public bool HasAnyCells
            => _List.Any();

        #region IGeometricRegion Members

        public IEnumerable<ICellLocation> AllCellLocations()
            => _List.Select(_cLoc => _cLoc);

        public bool ContainsCell(ICellLocation location)
            => _List.Any(_cLoc => (_cLoc.Z == location.Z) && (_cLoc.Y == location.Y) && (_cLoc.X == location.X));

        public bool ContainsCell(int z, int y, int x)
            => ContainsCell(new CellPosition(z, y, x));

        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = (LowerZ * 5) + (UpperZ + 1 - LowerZ) * 2.5,
                Y = (LowerY * 5) + (UpperY + 1 - LowerY) * 2.5,
                X = (LowerX * 5) + (UpperX + 1 - LowerX) * 2.5
            };

        public IGeometricRegion Move(ICellLocation offset)
            => new CellList(from _loc in _List
                            select new CellLocation(_loc.Z + offset.Z, _loc.Y + offset.Y, _loc.X + offset.X), ZNarrowest, YNarrowest, XNarrowest);

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
            => this.IsCellUnboundAtFace(location, surfaceFace);

        #endregion
    }
}