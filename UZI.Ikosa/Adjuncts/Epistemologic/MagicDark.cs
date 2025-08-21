using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicDark : Adjunct, ILocatorZone, IPathDependent, IMonitorChange<IGeometricRegion>
    {
        public MagicDark(object source, double range)
            : base(source)
        {
            _Range = range;
            _Capture = null;
        }

        #region state
        private double _Range;
        private LocatorCapture _Capture;
        protected PlanarPresence _Planar;

        [NonSerialized, JsonIgnore]
        protected IList<LocalCellGroup> _LastGroups = null;
        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            RefreshZone();
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            RemoveZone();
            NotifyGroups();
        }

        public double Range => _Range;
        public LocatorCapture LocatorCapture => _Capture;

        public override object Clone()
            => new MagicDark(Source, Range);

        #region ILocatorZone Members

        public void Start(Locator locator) { Capture(locator); }
        public void End(Locator locator) { Release(locator); }
        public void Enter(Locator locator) { Capture(locator); }
        public void Exit(Locator locator) { Release(locator); }

        public void Capture(Locator locator)
        {
            if (locator.ICore is ICoreObject _obj)
            {
                if (!_obj.Adjuncts.OfType<DarknessShrouded>().Any(_a => _a.Source == Source))
                {
                    _obj.AddAdjunct(new DarknessShrouded(Source));
                }
            }
        }

        public void Release(Locator locator)
        {
            if (locator.ICore is ICoreObject _obj)
            {
                foreach (var _adj in (from _a in _obj.Adjuncts.OfType<DarknessShrouded>()
                                      where _a.Source == Source
                                      select _a).ToList())
                {
                    _adj.Eject();
                }
            }
        }

        public void MoveInArea(Locator locator, bool followOn) { }

        #endregion

        // IPathDependent Members
        public void PathChanged(Pathed source)
        {
            // see if we are still locatable
            if (source is Located)
            {
                RefreshZone();
            }
        }

        private void RefreshZone()
        {
            // remove previous zone
            RemoveZone();

            // create new zone
            var _loc = Anchor.GetLocated()?.Locator;
            if (_loc != null)
            {
                // light on ethereal locators contribute nothing
                _Planar = _loc.PlanarPresence;
                if (_Planar.HasMaterialPresence())
                {
                    // define new capture
                    _Capture = new LocatorCapture(_loc.MapContext, this,
                        new Geometry(new SphereBuilder(Convert.ToInt32(Range / 5)), _loc, false), _loc,
                        this, true, _loc.PlanarPresence);

                    _Capture.Geometry.AddChangeMonitor(this);
                }
            }

            // notify groups
            NotifyGroups();
        }

        private void RemoveZone()
        {
            // remove capture
            _Capture?.Geometry?.RemoveChangeMonitor(this);
            _Capture?.RemoveZone();
            _Capture = null;
        }

        private void NotifyGroups()
        {
            var _loc = Anchor.GetLocated()?.Locator;
            if ((_loc != null) && _loc.PlanarPresence.HasMaterialPresence())
            {
                var _region = _Capture?.Geometry?.Region;
                var _newGroups = new List<LocalCellGroup>();
                _newGroups.AddRange(_loc.Map.RoomIndex.GetRooms(_region));
                _newGroups.AddRange(_loc.Map.Backgrounds.All().Where(_bg => _bg.IsOverlapping(_region)));
                var _groups = _newGroups.ToList();
                if (_LastGroups != null)
                {
                    _groups = _groups.Union(_LastGroups).Distinct().ToList();
                }
                var _notifiers = _groups
                    .SelectMany(_g => _g.NotifyLighting())
                    .Distinct()
                    .ToList();

                _LastGroups = _newGroups;
                AwarenessSet.RecalculateAllSensors(_loc.Map, _notifiers, false);
            }
            else if (_LastGroups != null)
            {
                // notify lighting
                var _notifiers = _LastGroups
                    .SelectMany(_g => _g.NotifyLighting())
                    .Distinct()
                    .ToList();
                var _map = _LastGroups.FirstOrDefault()?.Map;

                // clear last light
                _LastGroups = null;

                AwarenessSet.RecalculateAllSensors(_map, _notifiers, false);
            }

        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            NotifyGroups();
        }
    }
}
