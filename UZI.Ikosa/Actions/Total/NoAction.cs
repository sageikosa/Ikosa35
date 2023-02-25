using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class NoAction : ActionBase
    {
        public NoAction(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Total), false, false, orderKey)
        {
        }

        public override string Key => @"FullRound.NoAction";
        public override string DisplayName(CoreActor actor) => @"No Action";
        public override string Description => @"Do nothing";
        public override bool IsMental => true;
        protected override CoreStep OnPerformActivity(CoreActivity activity) { return null; }
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity) { yield break; }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Nothing", activity.Actor, observer);
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
