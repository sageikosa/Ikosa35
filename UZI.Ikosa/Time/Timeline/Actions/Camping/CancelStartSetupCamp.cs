using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class CancelStartSetupCamp : ActionBase
    {
        public CancelStartSetupCamp(DecideCampGroup group, string orderKey)
            : base(group, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.CancelStartSetupCamp";
        public override string DisplayName(CoreActor actor) => @"Cancel Start Setup Camp";

        public DecideCampGroup StartCampGroup => ActionSource as DecideCampGroup;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Starting Setup Camp", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // eject our intention to start a camp
            activity?.Actor?.Adjuncts.OfType<DecideCampMember>()
                .FirstOrDefault(_m => _m.DecideCampGroup == StartCampGroup)?.Eject();
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
