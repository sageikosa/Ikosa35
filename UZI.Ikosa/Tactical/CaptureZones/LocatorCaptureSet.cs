using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Set of zones that may react to locator position changes within
    /// </summary>
    [Serializable]
    public class LocatorCaptureSet : IMonitorChange<Activation>
    {
        public LocatorCaptureSet(MapContext context)
        {
            _Zones = new Collection<LocatorCapture>();
            MapContext = context;
        }

        public MapContext MapContext { get; private set; }

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
            var _zone = sender as LocatorCapture;
            if (args.NewValue.IsActive)
                _zone.StartAll();
            else
                _zone.EndAll();
        }
        #endregion

        private Collection<LocatorCapture> _Zones;

        #region public void Add(LocatorCapture zone)
        public void Add(LocatorCapture zone)
        {
            if (!_Zones.Contains(zone))
            {
                zone.AddChangeMonitor(this);
                _Zones.Add(zone);
                if (zone.IsActive)
                {
                    zone.StartAll();
                }
            }
            return;
        }
        #endregion

        #region public void Remove(LocatorCapture zone)
        public void Remove(LocatorCapture zone)
        {
            if (_Zones.Contains(zone))
            {
                _Zones.Remove(zone);
            }
            zone?.RemoveChangeMonitor(this);
            return;
        }
        #endregion

        #region public IEnumerable<LocatorCapture> AllCaptures()
        /// <summary>
        /// Enumerates all locator captures
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LocatorCapture> AllCaptures()
        {
            return _Zones.AsEnumerable();
        }
        #endregion

        /// <summary>Finds LocatorCaptures where the Capturer is the given ILocatorZone</summary>
        public IEnumerable<LocatorCapture> FindCapturer(ILocatorZone capturer)
        {
            return _Zones.Where(_zone => _zone.Capturer == capturer);
        }
    }
}
