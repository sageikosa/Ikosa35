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
    public class CompleteTrade : ActionBase
    {
        public CompleteTrade(IActionSource source, string orderKey)
            : base(source, new ActionTime(Minute.UnitFactor, TimeType.TimelineScheduling), false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.CompleteTrade";
        public override string DisplayName(CoreActor actor) => @"Trading";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Trading", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            throw new NotImplementedException();
        }
    }
}
