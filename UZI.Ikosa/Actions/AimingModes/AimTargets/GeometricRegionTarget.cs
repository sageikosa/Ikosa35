using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class GeometricRegionTarget : AimTarget
    {
        public GeometricRegionTarget(string key, IGeometricRegion target, MapContext mapContext)
            : base(key, null)
        {
            _Region = target;
            _MapContext = mapContext;
        }

        private IGeometricRegion _Region;
        public IGeometricRegion Region { get { return _Region; } }

        private MapContext _MapContext;
        public MapContext MapContext { get { return _MapContext; } }

        public override AimTargetInfo GetTargetInfo()
        {
            var _info = ToInfo<CubicTargetInfo>();
            _info.Cubic = new CubicInfo();
            _info.Cubic.SetCubicInfo(this.Region);
            return _info;
        }
    }
}
