using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Used to add ammo to an existing bundle or container
    /// </summary>
    [Serializable]
    public class ConsolidateAmmo<AmmoType, ProjectileWeapon> : ActionBase
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        /// <summary>
        /// Used to add ammo to an existing bundle or container
        /// </summary>
        public ConsolidateAmmo(IAmmunitionTypedBundle<AmmoType, ProjectileWeapon> bundle, ActionTime cost, string orderKey)
            : base(bundle, cost, true, false, orderKey)
        {
        }

        public IAmmunitionTypedBundle<AmmoType, ProjectileWeapon> TypedBundle
            => Source as IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>;

        public override string Key => @"Ammo.Consolidate";
        public override string DisplayName(CoreActor actor) => @"Consolidate Ammunition";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Consolidate Ammo", activity.Actor, observer);

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            // bundle must be able to hold something
            if ((TypedBundle?.Capacity ?? int.MaxValue) <= (TypedBundle?.Count ?? int.MaxValue))
                return new ActivityResponse(false);

            return base.OnCanPerformActivity(activity);
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                if (((activity.Targets[0] as ValueTarget<QuantitySelectTargetInfo>)?.Value.Selector is QuantitySelectorInfo _quantSelect)
                    && (_quantSelect.CoreInfo is AmmoInfo _ammoInfo))
                {
                    // tactically accessible containers?
                    var _container = (from _cont in _critter.GetReachable<IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>>()
                                      where _cont.ID == _ammoInfo.BundleID
                                      let _ammo = _cont.GetAmmunition(_critter, _ammoInfo.InfoIDs)
                                      where _ammo != null
                                      select _cont).FirstOrDefault();
                    if (_container != null)
                    {
                        // extract
                        var _bundle = _container.Extract(_critter, (_ammoInfo.InfoIDs, _quantSelect.CurrentSelection));

                        // merge
                        TypedBundle.Merge(_bundle);
                    }
                }
            }

            // register the activity
            var _register = new RegisterActivityStep(activity, Budget);
            // TODO: following steps if needed
            return _register;
        }

        #region private IEnumerable<QuantitySelectorInfo> AmmoTypes(CoreActivity activity)
        private IEnumerable<QuantitySelectorInfo> AmmoTypes(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // available capacity left
                var _avail = (TypedBundle?.Capacity ?? int.MaxValue) - (TypedBundle?.Count ?? 0);

                // nearby containers (excluding container consolidating into)
                foreach (var _info in from _container in _critter.GetReachable<IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>>()
                                      where _container != TypedBundle
                                      from _set in _container.AmmunitionInfos(_critter)
                                      select _set)
                {
                    yield return new QuantitySelectorInfo
                    {
                        CoreInfo = _info,
                        CurrentSelection = 0,
                        MinimumSelection = 1,
                        MaximumSelection = _avail < _info.Count ? _avail : _info.Count
                    };
                }
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new QuantitySelectAim(@"Source", @"Ammo to Include", FixedRange.One, FixedRange.One, AmmoTypes(activity));
            yield break;
        }
    }
}
