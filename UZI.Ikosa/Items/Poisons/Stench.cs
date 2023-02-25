using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items
{
    /// <summary>Base stench adjunct</summary>
    [Serializable]
    public abstract class Stench : Adjunct, ILocatorZone, IPathDependent, IMonitorChange<Size>, IMonitorChange<IGeometricSize>
    {
        /// <summary>Base stench adjunct</summary>
        protected Stench(IPoisonProvider source, IStenchGeometryBuilderFactory factory)  // TODO: range by calculation
            : base(source)
        {
            _Factory = factory;
            _Capture = null;
        }

        #region data
        private IStenchGeometryBuilderFactory _Factory;
        private LocatorCapture _Capture;
        #endregion

        public IPoisonProvider PoisonProvider => Source as IPoisonProvider;
        public IStenchGeometryBuilderFactory Factory => _Factory;
        public LocatorCapture LocatorCapture => _Capture;

        // IPathDependent Members
        public void PathChanged(Pathed source)
        {
            // see if we are still locatable
            if (source is Located)
            {
                var _loc = Locator.FindFirstLocator(Anchor);
                if (_loc != null)
                {
                    CommissionZone();
                }
                else
                {
                    // remove capture
                    DecommissionZone();
                }
            }
        }

        protected virtual IGeometricSize SourceSize(Locator locator)
            => locator.NormalSize as IGeometricSize;

        #region private void RefreshZone()
        /// <summary>Refreshses an existing zone (due to changes)</summary>
        private void RefreshZone()
        {
            // remove capture
            LocatorCapture?.RemoveZone();

            // create new one
            var _loc = Locator.FindFirstLocator(Anchor);
            if (_loc != null)
            {
                // TODO: should have a spread instead of a geometry builder for the capture zone
                // new cubic capture
                _Capture = new LocatorCapture(_loc.MapContext, this, 
                    new Geometry(Factory.GetStenchGeometryBuilder(), _loc, false),
                    _loc, this, true, _loc.PlanarPresence);
            }
        }
        #endregion

        #region private void CommissionZone()
        /// <summary>Ensures a zone exists (when activated, or bound to setting)</summary>
        private void CommissionZone()
        {
            // ensure stench zone exists
            var _locator = Locator.FindFirstLocator(Anchor);
            if (_locator != null)
            {
                // watch for locator size changes (which may differ from creature size)
                _locator.AddChangeMonitor(this);
            }

            // refresh the zone
            RefreshZone();

            // watch for size changes
            // NOTE: if body changes, items are unslotted and reslotted, so don't need to watch body itself
            (Anchor as Creature)?.Body.Sizer.AddChangeMonitor(this);
            (Anchor as ObjectBase)?.Sizer.AddChangeMonitor(this);
            (Anchor as ItemBase)?.Sizer.AddChangeMonitor(this);
        }
        #endregion

        #region private void DecommissionZone()
        /// <summary>Ensures a zone (and map monitoring) are torn down</summary>
        private void DecommissionZone()
        {
            // stop looking at size
            (LocatorCapture?.Origin as Locator)?.RemoveChangeMonitor(this);

            // remove capture
            LocatorCapture?.RemoveZone();
            _Capture = null;

            // stop watching creature size
            (Anchor as Creature)?.Body.Sizer.RemoveChangeMonitor(this);
            (Anchor as ObjectBase)?.Sizer.RemoveChangeMonitor(this);
            (Anchor as ItemBase)?.Sizer.RemoveChangeMonitor(this);
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            CommissionZone();
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            DecommissionZone();
            base.OnDeactivate(source);
        }
        #endregion

        #region IMonitorChange<Size> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            RefreshZone();
        }
        #endregion

        #region IMonitorChange<IGeometricSize> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricSize> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricSize> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricSize> args)
        {
            RefreshZone();
        }
        #endregion

        #region ILocatorZone
        private void TryPoison(Locator locator)
        {
            // only try with creatures
            if (locator?.Chief is Creature _critter)
            {
                var _poison = PoisonProvider.GetPoison();

                // do not even try to poison self
                if (_critter.ID != _poison.SourceID)
                {
                    // do no allow poisoning if already poisoned by this source
                    if (!_critter.Adjuncts.OfType<Poisoned>().Any(_p => _p.Poison.SourceID == _poison.SourceID))
                    {
                        // TODO: validate line of effect or in same room?
                        _critter.AddAdjunct(new Poisoned(_poison));
                    }
                }
            }
        }

        public void Start(Locator locator)
        {
            TryPoison(locator);
        }

        public void End(Locator locator)
        {
        }

        public void Enter(Locator locator)
        {
            TryPoison(locator);
        }

        public void Exit(Locator locator)
        {
        }

        public void Capture(Locator locator)
        {
            TryPoison(locator);
        }

        public void Release(Locator locator)
        {
        }

        public void MoveInArea(Locator locator, bool followOn)
        {
            TryPoison(locator);
        }

        #endregion
    }

    public interface IStenchGeometryBuilderFactory
    {
        IGeometryBuilder GetStenchGeometryBuilder();
    }
}
