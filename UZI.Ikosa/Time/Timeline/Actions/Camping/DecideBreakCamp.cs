using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DecideBreakCamp : ActionBase
    {
        public DecideBreakCamp(ActionTime actionTime, string orderKey)
            : base(null, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.DecideBreakCamp";
        public override string DisplayName(CoreActor actor) => @"Decide Break Camp";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Deciding Break Camp", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
