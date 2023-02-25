using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class StartRest : ActionBase
    {
        public StartRest(IActionSource group, ActionTime actionTime, string orderKey)
            : base(group, actionTime, true, false, orderKey)
        {
        }

        public override string Key => @"Timeline.StartRest";
        public override string DisplayName(CoreActor actor) => @"Start Rest";

        private IEnumerable<OptionAimOption> HoursToRest
            => Enumerable.Range(1, 8).Union(24.ToEnumerable())
            .Select(_h => new OptionAimValue<int>
            {
                Description = $@"Rest for {_h} hours",
                Key = _h.ToString(),
                Name = _h.ToString(),
                Value = _h
            });

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Hours", @"Hours to Rest", true, FixedRange.One, FixedRange.One, HoursToRest);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Starting Rest", activity.Actor, observer);

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
                    new Rest(ActionSource, @"200"),
                    activity.Targets);
                activity.EnqueueRegisterPreEmptively(Budget);
                return activity.GetActivityResultNotifyStep(@"Starting Rest");
            }
            else
            {
                return activity.GetActivityResultNotifyStep(@"Must Select Duration");
            }
        }
    }
}
