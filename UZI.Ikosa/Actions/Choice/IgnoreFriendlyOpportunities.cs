using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class IgnoreFriendlyOpportunities : ActionBase
    {
        public IgnoreFriendlyOpportunities(Creature critter)
            : base(critter, new ActionTime(TimeType.Free), false, false, @"200")
        {
        }

        public Creature Creature => Source as Creature;

        public override string Key => @"Opportunity.IgnoreFriendly";
        public override string DisplayName(CoreActor actor) => @"Opportunistic attacks on friends";
        public override bool IsStackBase(CoreActivity activity) => false;
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity) => false;
        public override bool IsMental => true;
        public override bool IsChoice => true;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _switch = (activity.Targets[0] as OptionTarget).Option;
            var _ignore = _switch?.Key.Equals(@"Ignore", StringComparison.OrdinalIgnoreCase) ?? false;
            if (_ignore != Creature.IgnoreFriendlyOpportunities)
            {
                Creature.IgnoreFriendlyOpportunities = _ignore;

                // status step
                return activity.GetActivityResultNotifyStep($@"Set to {(_ignore ? "ignore" : "prompt")}");
            }
            return activity.GetActivityResultNotifyStep(@"Already set to this value");
        }

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            if (!Creature.IgnoreFriendlyOpportunities)
            {
                yield return new OptionAimOption { Key = @"Prompt", Name = @"Prompt", Description = @"Prompt to attack friends", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Ignore", Name = @"Ignore", Description = @"No prompt to attack" };
            }
            else
            {
                yield return new OptionAimOption { Key = @"Ignore", Name = @"Ignore", Description = @"No prompt to attack", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Prompt", Name = @"Prompt", Description = @"Prompt to attack friends" };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Ignore", @"Prompt or ignore", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

    }
}
