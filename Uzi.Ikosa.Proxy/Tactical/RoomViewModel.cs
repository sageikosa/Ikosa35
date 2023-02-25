using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class RoomViewModel : IGeometricRegion, IGetCellStructure
    {
        public RoomViewModel(RoomInfo info)
        {
            _Info = info;
        }

        private RoomInfo _Info;
        private CellStructureInfo[,,] _Cells;

        public LocalCellGroupInfo Info => _Info;
        public RoomInfo RoomInfo { get { return _Info; } }

        public int LowerZ => RoomInfo?.LowerZ ?? 0;
        public int LowerY => RoomInfo?.LowerY ?? 0;
        public int LowerX => RoomInfo?.LowerX ?? 0;
        public int UpperZ => RoomInfo?.UpperZ ?? 0;
        public int UpperY => RoomInfo?.UpperY ?? 0;
        public int UpperX => RoomInfo?.UpperX ?? 0;

        public CellStructureInfo? GetContainedCellSpace(int z, int y, int x)
            => RoomInfo.ContainsCell(z, y, x) ? _Cells[z - LowerZ, y - LowerY, x - LowerX] : (CellStructureInfo?)null;

        #region public CellSpaceInfo this[int z, int y, int x] { get; internal set; }
        /// <summary>Gets a CellSpace using relative room coordinates</summary>
        public CellStructureInfo this[int z, int y, int x]
        {
            get
            {
                if (!BoundsCheck(z, y, x))
                    return new CellStructureInfo { CellSpace = null };

                return _Cells[z, y, x];
            }
            internal set
            {
                if (!BoundsCheck(z, y, x))
                    return;

                _Cells[z, y, x] = value;
            }
        }
        #endregion

        /// <summary>Gets a CellSpace ssing map coordinates</summary>
        public CellStructureInfo GetMapCell(int z, int y, int x)
        {
            return this[z - RoomInfo.Z, y - RoomInfo.Y, x - RoomInfo.X];
        }

        #region public bool BoundsCheck(int _z, int _y, int _x)
        /// <summary>Indicates the position falls within the extents of the drawing array</summary>
        public bool BoundsCheck(int _z, int _y, int _x)
        {
            if ((_z < 0) || (_y < 0) || (_x < 0))
            {
                return false;
            }
            if ((_z >= RoomInfo.ZHeight) || (_y >= RoomInfo.YLength) || (_x >= RoomInfo.XLength))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region public void RedrawMatrix()
        /// <summary>Allocates a new cell matrix and refills with cell space drawing items</summary>
        public void RedrawMatrix(LocalMapInfo map)
        {
            // clears (and resizes if necessary) the cell matrix
            _Cells = new CellStructureInfo[RoomInfo.ZHeight, RoomInfo.YLength, RoomInfo.XLength];
            var _cx = 0;
            for (var _z = 0; _z < RoomInfo.ZHeight; _z++)
                for (var _y = 0; _y < RoomInfo.YLength; _y++)
                    for (var _x = 0; _x < RoomInfo.XLength; _x++)
                    {
                        _Cells[_z, _y, _x] = map.GetCellSpaceInfo(RoomInfo.CellIDs[_cx]);
                        _cx++;
                    }
        }
        #endregion

        public Point3D GetPoint3D() => RoomInfo?.GetPoint() ?? new Point3D();
        public IEnumerable<ICellLocation> AllCellLocations() => RoomInfo?.AllCellLocations();
        public bool ContainsCell(int z, int y, int x) => RoomInfo?.ContainsCell(z, y, x) ?? false;
        public bool ContainsCell(ICellLocation location) => RoomInfo?.ContainsCell(location) ?? false;
        public IGeometricRegion Move(ICellLocation offset) => RoomInfo.Move(offset);

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
            => RoomInfo?.IsCellAtSurface(location, surfaceFace) ?? false;
    }
}
