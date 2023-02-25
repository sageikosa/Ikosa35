using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SwapShuriken : ActionBase
    {
        public SwapShuriken(ShurikenGrip grip, string orderKey)
            : base(grip, new ActionTime(Contracts.TimeType.FreeOnTurn), false, false, orderKey)
        {
        }

        public ShurikenGrip ShurikenGrip => ActionSource as ShurikenGrip;

        public override string Key => @"Shuriken.Swap";
        public override string DisplayName(CoreActor actor) => @"Swap Shuriken";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Swap", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(ShurikenGrip.Ammunition[0], observer);
            return _obs;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        private IEnumerable<CoreInfo> AmmoTypes(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // nearby containers
                foreach (var _info in from _container in _critter.GetReachable<IAmmunitionTypedBundle<Shuriken, ShurikenGrip>>()
                                      from _set in _container.AmmunitionInfos(_critter)
                                      select _set)
                {
                    yield return _info;
                }
            }
            yield break;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new CoreListAim(@"AmmoType", @"Ammunition", FixedRange.One, FixedRange.One, AmmoTypes(activity));
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((activity.Actor is Creature _critter)
                && (activity.Targets.OfType<ValueTarget<CoreInfo>>().FirstOrDefault()?.Value is AmmoInfo _shuriken))
            {
                // get ammunition already loaded
                Shuriken[] _replacement = null;
                if (ShurikenGrip.IsLoaded)
                    _replacement = ShurikenGrip.Ammunition;

                // tactically accessible containers?
                var _selector = (from _container in _critter.GetReachable<IAmmunitionTypedBundle<Shuriken, ShurikenGrip>>()
                                 where _container.ID == _shuriken.BundleID
                                 let _ammo = _container.GetAmmunition(_critter, _shuriken.InfoIDs)
                                 where _ammo != null
                                 select new
                                 {
                                     Container = _container,
                                     Shuriken = _ammo
                                 }).FirstOrDefault();
                if (_selector != null)
                {
                    // load up selected ammunition
                    ShurikenGrip.Ammunition = new Shuriken[] { _selector.Shuriken.Clone() as Shuriken };
                    _selector.Container.Use(_selector.Shuriken);

                    // stash previously loaded ammo back in the container
                    if (_replacement != null)
                    {
                        // just in case multiple were loaded...
                        foreach (var _rep in _replacement)
                        {
                            var _store = _rep.ToAmmunitionBundle(@"Shuriken") as AmmunitionBundle<Shuriken, ShurikenGrip>;
                            var _remainder = _selector.Container.Merge(_store);
                            if (_remainder.Count > 0)
                                Drop.DoDrop(_critter, _remainder, this, true);
                        }
                    }
                }
            }

            // register the activity
            var _register = new RegisterActivityStep(activity, Budget);
            // TODO: following steps if needed
            return _register;
        }
    }
}
