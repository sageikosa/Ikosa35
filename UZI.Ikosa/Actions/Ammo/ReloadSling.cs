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
    [Serializable]
    public class ReloadSling : ActionBase
    {
        public ReloadSling(Sling sling, string orderKey)
            : base(sling, new ActionTime(sling.ReloadAction), true, false, orderKey)
        {
        }

        public ReloadSling(Sling sling, ActionTime actionCost, bool provokesMelee, string orderKey)
            : base(sling, actionCost, provokesMelee, false, orderKey)
        {
        }

        public Sling Sling => Source as Sling;

        public override string Key => @"Sling.Reload";
        public override string DisplayName(CoreActor actor) => $@"Reload {Sling.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            // if one slot is not used, need to check for a free hand or a sling ammo bundle/container
            if ((Sling.MainSlot == null) || (Sling.SecondarySlot == null))
            {
                // if no free hands...
                if (Sling.CreaturePossessor.Body.ItemSlots.GetFreeHand(null) == null)
                {
                    // not holding any sling ammo containers
                    if (!Sling.CreaturePossessor.Body.ItemSlots.HeldObjects
                        .Any(_o => _o is IAmmunitionTypedBundle<SlingAmmo, Sling>))
                    {
                        return new ActivityResponse(false);
                    }
                }
            }
            return base.OnCanPerformActivity(activity);
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Reload", activity.Actor, observer, ((activity.Targets[0] as OptionTarget).Option as OptionAimValue<SlingAmmo>).Value);
            _obs.Implement = GetInfoData.GetInfoFeedback(Sling, observer);
            return _obs;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                if ((activity.Targets[0] as ValueTarget<CoreInfo>).Value is AmmoInfo _sAmmo)
                {
                    // get ammunition already loaded
                    SlingAmmo[] _replacement = null;
                    if (Sling.IsLoaded)
                        _replacement = Sling.Ammunition;

                    // tactically accessible containers?
                    var _selector = (from _container in _critter.GetReachable<IAmmunitionTypedBundle<SlingAmmo, Sling>>()
                                     where _container.ID == _sAmmo.BundleID
                                     let _ammo = _container.GetAmmunition(_critter, _sAmmo.InfoIDs)
                                     where _ammo != null
                                     select new
                                     {
                                         Container = _container,
                                         Ammo = _ammo
                                     }).FirstOrDefault();
                    if (_selector != null)
                    {
                        // load up selected ammunition
                        Sling.Ammunition = new SlingAmmo[] { _selector.Ammo.Clone() as SlingAmmo };
                        _selector.Container.Use(_selector.Ammo);

                        // stash previously loaded ammo back in the container
                        if (_replacement != null)
                        {
                            // just in case multiple were loaded...
                            foreach (var _rep in _replacement)
                            {
                                var _store = _rep.ToAmmunitionBundle(_rep.OriginalName) as AmmunitionBundle<SlingAmmo, Sling>;
                                var _remainder = _selector.Container.Merge(_store);
                                if (_remainder.Count > 0)
                                    Drop.DoDrop(_critter, _remainder, this, true);
                            }
                        }
                    }
                }

                // done
                activity.IsActive = false;
            }
            return null;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new CoreListAim(@"AmmoType", @"Ammunition", FixedRange.One, FixedRange.One, AmmoTypes(activity.Actor));
            yield break;
        }

        private IEnumerable<CoreInfo> AmmoTypes(CoreActor actor)
        {
            if (actor is Creature _critter)
            {
                // nearby containers
                foreach (var _info in from _container in _critter.GetReachable<IAmmunitionTypedBundle<SlingAmmo, Sling>>()
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
