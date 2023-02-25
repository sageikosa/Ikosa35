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
    public class StartSleep : ActionBase
    {
        public StartSleep(IActionSource source, ActionTime actionTime, string orderKey)
            : base(source, actionTime, true, false, orderKey)
        {
        }

        public override string Key => @"Timeline.StartSleep";
        public override string DisplayName(CoreActor actor) => @"Start Sleep";

        private IEnumerable<OptionAimOption> HoursToSleep
            => Enumerable.Range(1, 8).Union(24.ToEnumerable())
            .Select(_h => new OptionAimValue<int>
            {
                Description = $@"Sleep for {_h} hours",
                Key = _h.ToString(),
                Name = _h.ToString(),
                Value = _h
            });

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Hours", @"Hours to Sleep", true, FixedRange.One, FixedRange.One, HoursToSleep);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Starting Sleep", activity.Actor, observer);

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
                    new Sleep(ActionSource, @"200"),
                    activity.Targets);
                activity.EnqueueRegisterPreEmptively(Budget);
                return activity.GetActivityResultNotifyStep(@"Starting Sleep");
            }
            else
            {
                return activity.GetActivityResultNotifyStep(@"Must Select Duration");
            }
        }
    }
}
