using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize.Contracts;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class LocationTarget : AimTarget
    {
        public LocationTarget(string key, LocationAimMode mode, ICellLocation location, MapContext mapContext)
            : base(key, null)
        {
            _Loc = new CellPosition(location);
            _Mode = mode;
            _MapContext = mapContext;
        }

        #region private data
        private ICellLocation _Loc;
        private LocationAimMode _Mode;
        private MapContext _MapContext;
        #endregion

        public ICellLocation Location => _Loc;
        public LocationAimMode LocationAimMode => _Mode;
        public MapContext MapContext => _MapContext;

        public Point3D SupplyPoint3D()
            => (_Mode == LocationAimMode.Cell
            ? _Loc?.GetPoint()
            : _Loc?.Point3D())
            ?? new Point3D();

        public override AimTargetInfo GetTargetInfo()
            => new LocationTargetInfo
            {
                CellInfo = new CellInfo(Location),
                Key = Key,
                TargetID = Target?.ID
            };

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public void AddChangeMonitor(IMonitorChange<Point3D> monitor)
        {
        }

        public void RemoveChangeMonitor(IMonitorChange<Point3D> monitor)
        {
        }
    }
}
