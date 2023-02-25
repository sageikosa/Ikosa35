using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Choose ammunition from a slotted ammunition container to load the crossbow, swaps with current ammo if already loaded
    /// </summary>
    [Serializable]
    public class ReloadCrossbow : ActionBase
    {
        /// <summary>
        /// Choose ammunition from a slotted ammunition container to load the crossbow, swaps with current ammo if already loaded
        /// </summary>
        public ReloadCrossbow(CrossbowBase crossBow, string orderKey)
            : base(crossBow, new ActionTime(crossBow.ReloadAction), true, false, orderKey)
        {
        }

        public CrossbowBase CrossBow => Source as CrossbowBase;
        public override string Key => @"CrossBow.Reload";
        public override string DisplayName(CoreActor actor) => $@"Reload {CrossBow.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            // if one slot is not used, need to check for a free hand or a crossbow ammo bundle/container
            if ((CrossBow.MainSlot == null) || (CrossBow.SecondarySlot == null))
            {
                // if no free hands...
                if (CrossBow.CreaturePossessor.Body.ItemSlots.GetFreeHand(null) == null)
                {
                    // not holding any sling ammo containers
                    if (!CrossBow.CreaturePossessor.Body.ItemSlots.HeldObjects
                        .Any(_o => _o is IAmmunitionTypedBundle<CrossbowBolt, CrossbowBase>))
                    {
                        return new ActivityResponse(false);
                    }
                }
            }
            return base.OnCanPerformActivity(activity);
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Reload", activity.Actor, observer, ((activity.Targets[0] as OptionTarget).Option as OptionAimValue<CrossbowBolt>).Value);
            _obs.Implement = GetInfoData.GetInfoFeedback(CrossBow, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                if ((activity.Targets[0] as ValueTarget<CoreInfo>).Value is AmmoInfo _bolt)
                {
                    // get ammunition already loaded
                    CrossbowBolt[] _replacement = null;
                    if (CrossBow.IsLoaded)
                        _replacement = CrossBow.Ammunition;

                    // tactically accessible containers?
                    var _selector = (from _container in _critter.GetReachable<IAmmunitionTypedBundle<CrossbowBolt, CrossbowBase>>()
                                     where _container.ID == _bolt.BundleID
                                     let _ammo = _container.GetAmmunition(_critter, _bolt.InfoIDs)
                                     where _ammo != null
                                     select new
                                     {
                                         Container = _container,
                                         Bolt = _ammo
                                     }).FirstOrDefault();

                    if (_selector != null)
                    {
                        // load up selected ammunition
                        CrossBow.Ammunition = new CrossbowBolt[] { _selector.Bolt.Clone() as CrossbowBolt };
                        _selector.Container.Use(_selector.Bolt);

                        // stash previously loaded ammo back in the container
                        if (_replacement != null)
                        {
                            // just in case multiple were loaded...
                            foreach (var _rep in _replacement)
                            {
                                var _store = _rep.ToAmmunitionBundle(@"Bolt") as AmmunitionBundle<CrossbowBolt, CrossbowBase>;
                                var _remainder = _selector.Container.Merge(_store);
                                if (_remainder.Count > 0)
                                    Drop.DoDrop(_critter, _remainder, this, true);
                            }
                        }
                    }
                }
            }

            // register the activity
            var _register = new RegisterActivityStep(activity, Budget);
            // TODO: following steps if needed
            return _register;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new CoreListAim(@"AmmoType", @"Ammunition", FixedRange.One, FixedRange.One, AmmoTypes(activity));
            yield break;
        }

        private IEnumerable<CoreInfo> AmmoTypes(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // nearby containers
                foreach (var _info in from _container in _critter.GetReachable<IAmmunitionTypedBundle<CrossbowBolt, CrossbowBase>>()
                                      from _set in _container.AmmunitionInfos(_critter)
                                      select _set)
                {
                    yield return _info;
                }
            }
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
