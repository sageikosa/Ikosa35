using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class SetupCamp : ActionBase
    {
        public SetupCamp(TeamGroup group, ActionTime actionTime, string orderKey)
            : base(group, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.SetupCamp";
        public override string DisplayName(CoreActor actor) => @"Setup Camp";

        public TeamGroup TeamGroup => ActionSource as TeamGroup;

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
            activity.EnqueueRegisterPreEmptively(Budget);
            return new SetupCampStep(activity);
        }
    }
}
