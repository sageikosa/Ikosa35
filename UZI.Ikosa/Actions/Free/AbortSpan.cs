using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class AbortSpan : ActionBase
    {
        public AbortSpan(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"AbortSpan";
        public override string DisplayName(CoreActor actor) => @"Abandon Current Action";

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Abort Action", activity.Actor, observer);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // remove the span adjunct
            var _spanAdj = activity.Actor.Adjuncts.OfType<SpanActionAdjunct>().FirstOrDefault();
            if (_spanAdj != null)
            {
                activity.Actor.RemoveAdjunct(_spanAdj);
            }
            return null;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
