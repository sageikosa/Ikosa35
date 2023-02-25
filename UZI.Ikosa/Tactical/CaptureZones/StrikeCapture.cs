using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Capture zone embedded in the MapContext, with a link to the weapon that provides the zone
    /// </summary>
    [Serializable]
    public class StrikeCapture : CaptureZone
    {
        public StrikeCapture(MapContext mapContext, StrikeZoneLink source, Geometry geometry, PlanarPresence planar)
            : base(mapContext, source, geometry, planar)
        {
        }

        protected override void OnRemoveZone()
        {
            MapContext.StrikeZones.Remove(this);
        }

        public StrikeZoneLink ZoneLink
            => Source as StrikeZoneLink;
    }
}
