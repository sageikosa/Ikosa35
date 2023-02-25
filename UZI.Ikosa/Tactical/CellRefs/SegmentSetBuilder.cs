using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SegmentSetBuilder : IGeometryBuilder
    {
        public SegmentSetBuilder(LocalMap map, IGeometricRegion sourceRegion, IGeometricRegion targetRegion,
            CellPosition offset, LocationAimMode aimMode, SegmentSetProcess process, PlanarPresence planar)
        {
            _Map = map;
            _Offset = offset;
            _AimMode = aimMode;
            _Process = process;
            _SourceRegion = sourceRegion;
            _TargetRegion = targetRegion;
            _Planar = planar;
        }

        #region data
        private LocalMap _Map;
        private CellPosition _Offset;
        private LocationAimMode _AimMode;
        private SegmentSetProcess _Process;
        private IGeometricRegion _SourceRegion;
        private IGeometricRegion _TargetRegion;
        private PlanarPresence _Planar;
        #endregion

        public LocalMap Map => _Map;
        public CellPosition Offset => _Offset;
        public LocationAimMode LocationAimMode => _AimMode;
        public SegmentSetProcess SegmentSetProcess => _Process;
        public IGeometricRegion SourceRegion => _SourceRegion;
        public IGeometricRegion TargetRegion => _TargetRegion;
        public PlanarPresence PlanarPresence => _Planar;

        public IGeometricRegion BuildGeometry(LocationAimMode aimMode, ICellLocation location)
        {
            var _start = aimMode.GetPoint3D(location);
            var _final = location.Add(Offset);
            var _end = LocationAimMode.GetPoint3D(_final);
            var _segSet = new SegmentSet(Map, _start, _end, SourceRegion, TargetRegion, new ITacticalInquiry[] { }, 
                SegmentSetProcess, PlanarPresence);
            return new CellList(_segSet.All().Select(_c => (ICellLocation)_c.ToCellPosition()), 0, 0, 0);
        }
    }
}
