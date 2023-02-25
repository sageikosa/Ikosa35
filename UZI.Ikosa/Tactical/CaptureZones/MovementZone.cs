using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Non-reactive zone that provides additional movement requirements, modifiers or restrictions</summary>
    [Serializable]
    public class MovementZone : CaptureZone
    {
        public MovementZone(MapContext context, object source, Geometry geom, PlanarPresence planar,
            params MovementZoneProperties[] properties)
            : base(context, source, geom, planar)
        {
            _Properties = properties.ToList();
        }

        protected override void OnRemoveZone()
        {
            MapContext.MovementZones.Remove(this);
        }

        private List<MovementZoneProperties> _Properties;
        public List<MovementZoneProperties> Properties { get { return _Properties; } }

        public bool AffectsMovement<MoveType>() where MoveType: MovementBase
        {
            return _Properties.Any(_p => _p.MoveType.Equals(typeof(MoveType)));
        }

        public IEnumerable<MovementZoneProperties> GetProperties<MoveType>() 
            where MoveType : MovementBase
        {
            return _Properties.Where(_p => _p.MoveType.Equals(typeof(MoveType)));
        }
    }
}
