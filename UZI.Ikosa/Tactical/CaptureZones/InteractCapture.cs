using System;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Defines a zone that can alter interactions that pass through it
    /// </summary>
    [Serializable]
    public class InteractCapture : CaptureZone, IAlterInteraction, IMonitorChange<Activation>
    {
        #region ctor()
        public InteractCapture(MapContext mapContext, object source, Geometry geometry,
            IAlterInteraction alterer, bool active, PlanarPresence planar)
            : base(mapContext, source, geometry, planar)
        {
            _Alterer = alterer;
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, active));
            alterer.AddChangeMonitor(this);
            mapContext.InteractTransitZones.Add(this);
        }

        public InteractCapture(MapContext mapContext, object source, Geometry geometry, Intersection origin,
            IAlterInteraction alterer, bool active, PlanarPresence planar)
            : base(mapContext, source, geometry, planar, origin)
        {
            _Alterer = alterer;
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, active));
            alterer.AddChangeMonitor(this);
            mapContext.InteractTransitZones.Add(this);
        }
        #endregion

        #region state
        private ChangeController<Activation> _ACtrl;
        private IAlterInteraction _Alterer;
        #endregion

        public IAlterInteraction Alterer => _Alterer;

        // IAlterInteraction Members
        public bool WillDestroyInteraction(Interaction workSet, ITacticalContext context)
            => _Alterer.WillDestroyInteraction(workSet, context);

        public bool CanAlterInteraction(Interaction workSet)
            => _Alterer.CanAlterInteraction(workSet);

        public bool IsActive
            => _Alterer.IsActive;

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

        // INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnRemoveZone()
        {
            MapContext.InteractTransitZones.Remove(this);
        }

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
    }
}
