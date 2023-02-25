using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Source for strike capture, and connects to a weapon via StrikeZoneMember</summary>
    [Serializable]
    public class StrikeZoneLink : AdjunctGroup
    {
        #region construction
        /// <summary>Source for strike capture, and connects to a weapon via StrikeZoneMember</summary>
        public StrikeZoneLink()
            : base(null)
        {
        }
        #endregion

        public StrikeCapture StrikeCapture
            => StrikeZoneMaster?.Locator?.MapContext.StrikeZones.FindCaptureByZoneLink(this);

        public StrikeZoneMaster StrikeZoneMaster
            => Members.FirstOrDefault() as StrikeZoneMaster;

        /// <summary>Assigns a new ID to the StrikeZoneLink (used when BindingToSetting as an injected object)</summary>
        internal void ResetMyID()
            => ResetID();

        public override void ValidateGroup() { }
    }

    /// <summary>Attached to a melee weapon that has been wielded</summary>
    [Serializable]
    public class StrikeZoneMaster : GroupMasterAdjunct, IMonitorChange<Size>, IMonitorChange<IGeometricSize>
    {
        #region construction
        /// <summary>Attached to a melee weapon that has been wielded</summary>
        public StrikeZoneMaster(IMeleeWeapon source, StrikeZoneLink group)
            : base(source, group)
        {
        }
        #endregion

        public StrikeZoneLink ZoneLink => Group as StrikeZoneLink;
        public IMeleeWeapon Weapon => Anchor as IMeleeWeapon;

        private StrikeZoneLink MasteredLink => MasterGroup as StrikeZoneLink;

        public Locator Locator
            => Weapon?.GetLocated()?.Locator;

        #region private void RefreshStrikeZone()
        /// <summary>Refreshses an existing strike zone (due to changes)</summary>
        private void RefreshStrikeZone()
        {
            // remove opportunistic capture
            var _loc = Locator;
            if (_loc != null)
            {
                var _mapCtx = _loc.MapContext;
                _mapCtx.StrikeZones.FindCaptureByZoneLink(ZoneLink)?.RemoveZone();

                // create new one
                var _geom = Weapon.GetStrikeZone();
                if (_geom != null)
                {
                    // NOTE: ignoring force attibuted weapons
                    var _capture = new StrikeCapture(_mapCtx, MasteredLink, _geom, _loc.PlanarPresence);
                    _mapCtx.StrikeZones.Add(_capture);
                }
            }
        }
        #endregion

        #region private void CommissionStrikeZone()
        /// <summary>Ensures a strike zone exists (when activated, or bound to setting)</summary>
        private void CommissionStrikeZone()
        {
            // ensure strike zone exists
            var _locator = Locator.FindFirstLocator(Weapon.Possessor);
            if (_locator != null)
            {
                // watch for locator size changes (which may differ from creature size)
                _locator.AddChangeMonitor(this);
            }

            // refresh the zone
            RefreshStrikeZone();

            if (Weapon.Possessor is Creature _critter)
            {
                // watch for creature size changes
                // NOTE: if body changes, items are unslotted and reslotted, so don't need to watch body itself
                _critter.Body.Sizer.AddChangeMonitor(this);
            }
        }
        #endregion

        #region private void DecommissionStrikeZone()
        /// <summary>Ensures a strike zone (and map monitoring) are torn down</summary>
        private void DecommissionStrikeZone()
        {
            // remove stuff
            var _loc = Locator;
            if (_loc != null)
            {
                var _locator = Locator.FindFirstLocator(Weapon.Possessor);
                if (_locator != null)
                    _locator.RemoveChangeMonitor(this);

                // remove opportunistic capture
                _loc.MapContext.StrikeZones.FindCaptureByZoneLink(ZoneLink)?.RemoveZone();
            }

            // stop watching creature size
            (Weapon.Possessor as Creature)?.Body.Sizer.RemoveChangeMonitor(this);
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            CommissionStrikeZone();
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            DecommissionStrikeZone();
            base.OnDeactivate(source);
        }
        #endregion

        #region IMonitorChange<Size> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            // strike zone member may have been ejected prior to this...
            if (Weapon != null)
                RefreshStrikeZone();
        }
        #endregion

        #region IMonitorChange<IGeometricSize> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricSize> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricSize> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricSize> args)
        {
            // strike zone member may have been ejected prior to this...
            if (Weapon != null)
                RefreshStrikeZone();
        }
        #endregion

        #region public override void BindToSetting()
        public override void BindToSetting()
        {
            // ensure group adjunct present
            MasteredLink.ResetMyID();
            base.BindToSetting();
            CommissionStrikeZone();
        }
        #endregion

        #region public override void UnbindFromSetting()
        public override void UnbindFromSetting()
        {
            DecommissionStrikeZone();
            base.UnbindFromSetting();
        }
        #endregion

        public override object Clone()
            => new StrikeZoneMaster(Weapon, new StrikeZoneLink());
    }
}