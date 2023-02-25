using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Set of zones that define striking ranges for melee weapons
    /// </summary>
    [Serializable]
    public class StrikeCaptureSet
    {
        public StrikeCaptureSet()
        {
            _Zones = new Collection<StrikeCapture>();
        }

        private Collection<StrikeCapture> _Zones;

        #region public void Add(StrikeCapture zone)
        public void Add(StrikeCapture zone)
        {
            if (!_Zones.Contains(zone))
            {
                _Zones.Add(zone);
            }
            return;
        }
        #endregion

        #region public void Remove(StrikeCapture zone)
        public void Remove(StrikeCapture zone)
        {
            if (_Zones.Contains(zone))
            {
                _Zones.Remove(zone);
            }
            return;
        }
        #endregion

        /// <summary>Enumerates all locator captures</summary>
        public IEnumerable<StrikeCapture> AllCaptures
            => _Zones.Select(_z => _z);

        /// <summary>
        /// Finds StrikeCapture where the ZoneLink is the given StrikeZoneLink
        /// </summary>
        public StrikeCapture FindCaptureByZoneLink(StrikeZoneLink zoneLink)
        {
            return _Zones.FirstOrDefault(_zone => _zone.ZoneLink == zoneLink);
        }
    }
}
