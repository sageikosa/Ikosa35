using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class KeepWatch : ActionBase
    {
        public KeepWatch(IActionSource source, string orderKey)
            : base(source, new ActionTime(Hour.UnitFactor, TimeType.TimelineScheduling), false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.KeepingWatch";
        public override string DisplayName(CoreActor actor) => @"Keeping Watch";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Alert", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            if (activity?.GetFirstTarget<OptionTarget>(@"Hours")?.Option is OptionAimValue<int> _hours)
            {
                if (activity.Actor is Creature _critter)
                {
                    var _map = _critter.Setting as LocalMap;
                    //var _recovery = _map.ContextSet.AdjunctGroups.Singleton<RecoveryRest>(() => new RecoveryRest());
                    //if (!_critter.Adjuncts.OfType<SleepEffect>().Any(_s => (Type)_s.Source == GetType()))
                    //{
                    //    _critter.AddAdjunct(new SleepEffect(GetType()));
                    //}
                    //if (!_critter.HasAdjunct<MentalRest>())
                    //{
                    //    _critter.AddAdjunct(new MentalRest(_map.CurrentTime, _map.ID, _recovery));
                    //}

                    //if (!_critter.HasAdjunct<NaturalHealing>())
                    //{
                    //    _critter.AddAdjunct(new NaturalHealing(_map.CurrentTime, _hours.Value >= 24, _map.ID, _recovery));
                    //}

                    // decrease hours count
                    _hours.Value--;
                    if (_hours.Value > 0)
                    {
                        // continue
                        Budget.NextActivity = new CoreActivity(
                            _critter,
                            new KeepWatch(ActionSource, OrderKey),
                            activity.Targets);
                    }
                    return activity.GetActivityResultNotifyStep($@"Kept watch 1 hour");
                }
            }
            return activity.GetActivityResultNotifyStep(@"Improper keep watch parameters");
        }
    }
}
