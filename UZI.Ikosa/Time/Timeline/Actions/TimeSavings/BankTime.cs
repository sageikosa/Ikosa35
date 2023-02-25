using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class BankTime : ActionBase
    {
        public BankTime(IActionSource source, string orderKey)
            : base(source, new ActionTime(Hour.UnitFactor, TimeType.TimelineScheduling), false, false, orderKey)
        {
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override string Key => @"Timeline.Savings";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override string DisplayName(CoreActor observer)
            => @"Bank time for general activities";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Banking Time", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            if (activity.Actor is Creature _critter)
            {
                if (!_critter.HasActiveAdjunct<ActionTimeKeeper>())
                {
                    _critter.AddAdjunct(new ActionTimeKeeper(GetType()));
                }
                return activity.GetActivityResultNotifyStep($@"Banked 1 hour");
            }

            return activity.GetActivityResultNotifyStep(@"Not a creature");
        }
    }
}
