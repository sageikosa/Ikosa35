using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Used to create a new bundle from an existing bundle or container
    /// </summary>
    [Serializable]
    public class ExtractAmmo<AmmoType, ProjectileWeapon> : ActionBase
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        /// <summary>
        /// Used to create a new bundle from an existing bundle or container
        /// </summary>
        public ExtractAmmo(IAmmunitionTypedBundle<AmmoType, ProjectileWeapon> bundle, ActionTime cost, string orderKey)
            : base(bundle, cost, true, false, orderKey)
        {
        }

        public IAmmunitionTypedBundle<AmmoType, ProjectileWeapon> TypedBundle
            => Source as IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>;

        public override string Key => @"Ammo.Extract";
        public override string DisplayName(CoreActor actor) => @"Extract ammunition";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Extract Ammo", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // extract from "bundle", place in hand or drop
                if (((activity.Targets[0] as ValueTarget<QuantitySelectTargetInfo>)?.Value.Selector is QuantitySelectorInfo _quantSelect)
                    && (_quantSelect.CoreInfo is AmmoInfo _ammoInfo))
                {
                    // extract from sourced bundle into new bundle
                    var _bundle = TypedBundle.Extract(_critter, (_ammoInfo.InfoIDs, _quantSelect.CurrentSelection));
                    if (_bundle?.Count > 0)
                    {
                        // get first empty hand
                        var _emptyHand = _critter.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                            .FirstOrDefault(_s => _s.SlottedItem == null);
                        if (_emptyHand != null)
                        {
                            var _wrapper = HoldingSlot.GetHoldableItem(_bundle, _critter);

                            // slot the item
                            _wrapper.SetItemSlot(_emptyHand);

                            // ! slotted
                            if (_wrapper.MainSlot != _emptyHand)
                            {
                                Drop.DoDrop(_critter, _bundle, this, true);
                            }
                        }
                        else
                        {
                            // no empty hand
                            Drop.DoDrop(_critter, _bundle, this, true);
                        }
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
                // this container
                foreach (var _info in from _set in TypedBundle.AmmunitionInfos(_critter)
                                      select _set)
                {
                    yield return new QuantitySelectorInfo
                    {
                        CoreInfo = _info,
                        CurrentSelection = 0,
                        MinimumSelection = 1,
                        MaximumSelection = _info.Count
                    };
                }
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // NOTE: only allowing one selection for now
            // might be able to extract multiple types in the future
            yield return new QuantitySelectAim(@"Source", @"Ammo to Extract", FixedRange.One, FixedRange.One, AmmoTypes(activity));
            yield break;
        }
    }
}
