using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class WallSurfaceTarget : AimTarget
    {
        public WallSurfaceTarget(string key, ICellLocation location, AnchorFace anchorFace, MapContext mapContext)
            : base(key, null)
        {
            _Location = new CellLocation(location);
            _Face = anchorFace;
            _MapContext = mapContext;
        }

        #region data
        private CellLocation _Location;
        private MapContext _MapContext;
        private AnchorFace _Face;
        #endregion

        public AnchorFace AnchorFace => _Face;
        public CellLocation Location => _Location;
        public MapContext MapContext => _MapContext;

        public override AimTargetInfo GetTargetInfo()
            => new WallSurfaceTargetInfo
            {
                AnchorFace = (int)AnchorFace,
                CellInfo = new CellInfo(Location),
                Key = Key,
                TargetID = Target?.ID
            };
    }
}
