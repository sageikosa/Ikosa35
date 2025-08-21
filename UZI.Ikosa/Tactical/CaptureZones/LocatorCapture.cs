using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core;
using System.ComponentModel;
using Uzi.Visualize;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Hooks tactical geometry to consequences for locators.  
    /// Informs the MapContext of activation/de-activation, and tracks the same from its ILocatorZone reference.
    /// </summary>
    [Serializable]
    public class LocatorCapture : CaptureZone, ILocatorZone, IMonitorChange<Activation>, IMonitorChange<IGeometricRegion>
    {
        #region ctor()
        public LocatorCapture(MapContext mapContext, object source, Geometry geometry, 
            ILocatorZone capturer, bool active, PlanarPresence planar) :
            base(mapContext, source, geometry, planar)
        {
            _Capturer = capturer;
            _Contents = [];
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, active));
            capturer.AddChangeMonitor(this);
            mapContext.LocatorZones.Add(this);
            geometry.AddChangeMonitor(this);
        }

        public LocatorCapture(MapContext mapContext, object source, Geometry geometry, IGeometryAnchorSupplier origin, 
            ILocatorZone capturer, bool active, PlanarPresence planar) :
            base(mapContext, source, geometry, planar, origin)
        {
            _Capturer = capturer;
            _Contents = [];
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, active));
            capturer.AddChangeMonitor(this);
            mapContext.LocatorZones.Add(this);
            geometry.AddChangeMonitor(this);
        }
        #endregion

        #region state
        private ILocatorZone _Capturer;
        private Collection<Locator> _Contents;
        private ChangeController<Activation> _ACtrl;
        #endregion

        public ILocatorZone Capturer => _Capturer;

        #region public IEnumerable<IStep> StartAll()
        /// <summary>called automatically when the zone is initialized</summary>
        public void StartAll()
        {
            foreach (var _loc in MapContext.LocatorsInRegion(Geometry.Region, PlanarPresence))
            {
                // filters when an intersection point is provided
                if (ContainsGeometricRegion(_loc.GeometricRegion, _loc.ICore as ICoreObject, _loc.PlanarPresence))
                {
                    Start(_loc);
                }
            }
        }
        #endregion

        #region public void EndAll()
        /// <summary>Called when the zone is being shut down</summary>
        public void EndAll()
        {
            foreach (Locator _locator in Contents)
            {
                End(_locator);
            }
        }
        #endregion

        protected override void OnRemoveZone()
        {
            MapContext.LocatorZones.Remove(this);
        }

        /// <summary>Enumerates all zone contents</summary>
        public IEnumerable<Locator> Contents
            => _Contents.Select(_l => _l);

        #region ILocatorZone Members
        public void Start(Locator locator)
        {
            if (!_Contents.Contains(locator))
            {
                _Contents.Add(locator);
                _Capturer.Start(locator);
            }
        }

        public void End(Locator locator)
        {
            if (_Contents.Contains(locator))
            {
                _Contents.Remove(locator);
                _Capturer.End(locator);
            }
        }

        public void Enter(Locator locator)
        {
            if (!_Contents.Contains(locator))
            {
                _Contents.Add(locator);
                _Capturer.Enter(locator);
            }
        }

        public void Exit(Locator locator)
        {
            if (_Contents.Contains(locator))
            {
                _Contents.Remove(locator);
                _Capturer.Exit(locator);
            }
        }

        public void Capture(Locator locator)
        {
            if (!_Contents.Contains(locator))
            {
                _Contents.Add(locator);
                _Capturer.Capture(locator);
            }
        }

        public void Release(Locator locator)
        {
            if (_Contents.Contains(locator))
            {
                _Contents.Remove(locator);
                _Capturer.Release(locator);
            }
        }

        public void MoveInArea(Locator locator, bool followOn)
        {
            if (_Contents.Contains(locator))
            {
                _Capturer.MoveInArea(locator, followOn);
            }
        }

        public bool IsActive
            => _Capturer.IsActive;
        #endregion

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ACtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ACtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
            // if the activation of the capturer changes, so follows this zone
            if (args.NewValue.IsActive != args.OldValue.IsActive)
            {
                _ACtrl.DoValueChanged(args.NewValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            }
        }
        #endregion

        #region IMonitorChange<IGeometricRegion> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            var _current = MapContext.LocatorsInRegion(Geometry.Region, PlanarPresence)
                .Where(_loc => ContainsGeometricRegion(_loc.GeometricRegion, _loc.ICore as ICoreObject, _loc.PlanarPresence)).ToList();
            var _releasing = Contents.Where(_c => !_current.Contains(_c)).ToList();
            var _capturing = _current.Where(_c => !Contents.Contains(_c)).ToList();
            foreach (var _rel in _releasing)
            {
                Release(_rel);
            }
            foreach (var _cap in _capturing)
            {
                Capture(_cap);
            }
        }

        #endregion
    }
}
