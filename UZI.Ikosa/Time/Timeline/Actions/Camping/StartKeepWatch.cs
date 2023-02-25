using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class StartKeepWatch : ActionBase
    {
        public StartKeepWatch(ActionTime actionTime, string orderKey)
            : base(null, actionTime, true, false, orderKey)
        {
        }

        public override string Key => @"Timeline.StartKeepWatch";
        public override string DisplayName(CoreActor actor) => @"Start Keeping Watch";

        private IEnumerable<OptionAimOption> HoursToWatch
            => Enumerable.Range(1, 8)
            .Select(_h => new OptionAimValue<int>
            {
                Description = $@"Sleep for {_h} hours",
                Key = _h.ToString(),
                Name = _h.ToString(),
                Value = _h
            });

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Hours", @"Hours to Watch", true, FixedRange.One, FixedRange.One, HoursToWatch);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Starting Keep Watch", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity?.GetFirstTarget<OptionTarget>(@"Hours")?.Option is OptionAimValue<int>)
            {
                Budget.NextActivity = new CoreActivity(
                    activity.Actor,
                    new KeepWatch(ActionSource, @"200"),
                    activity.Targets);
                activity.EnqueueRegisterPreEmptively(Budget);
                return activity.GetActivityResultNotifyStep(@"Starting Keeping Watch");
            }
            else
            {
                return activity.GetActivityResultNotifyStep(@"Must Select Duration");
            }
        }
    }
}
