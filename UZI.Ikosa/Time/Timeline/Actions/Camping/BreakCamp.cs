using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class BreakCamp : ActionBase
    {
        public BreakCamp(CampingGroup group, ActionTime actionTime, string orderKey)
            : base(group, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.BreakCamp";
        public override string DisplayName(CoreActor actor) => @"Break Camp";

        public CampingGroup CampingGroup => Source as CampingGroup;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO: break camp...gather camping gear bound with group adjunct
            return null;
        }
    }
}
