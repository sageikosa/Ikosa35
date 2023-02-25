using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SmiteChoice : ActionBase
    {
        public SmiteChoice(SmiteTrait smiteTrait)
            : base(smiteTrait.Creature, new ActionTime(Contracts.TimeType.Free), false, false, @"200")
        {
            _Smite = smiteTrait;
        }

        #region data
        private SmiteTrait _Smite;
        #endregion

        public SmiteTrait SmiteTrait => _Smite;
        public override string Key => $@"Smite.{SmiteTrait.Alignment.NoNeutralString()}";
        public override string DisplayName(CoreActor actor) => $@"Smite {SmiteTrait.Alignment.NoNeutralString()}";
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
            var _smite = _switch?.Key.Equals(@"Smite", StringComparison.OrdinalIgnoreCase) ?? false;
            if (_smite != SmiteTrait.IsSmiting)
            {
                SmiteTrait.IsSmiting = _smite;

                // status step
                return activity.GetActivityResultNotifyStep($@"Set to {(_smite ? "smite" : "no smite")}");
            }
            return activity.GetActivityResultNotifyStep(@"Already set to this value");
        }

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options(CoreActor actor)
        {
            if (SmiteTrait.IsSmiting)
            {
                yield return new OptionAimOption { Key = @"Smite", Name = @"Smite", Description = DisplayName(actor), IsCurrent = true };
                yield return new OptionAimOption { Key = @"NoSmite", Name = @"No Smite", Description = @"Regular attack" };
            }
            else
            {
                yield return new OptionAimOption { Key = @"NoSmite", Name = @"No Smite", Description = @"Regular attack", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Smite", Name = @"Smite", Description = DisplayName(actor) };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Smite", @"Smite or no-smite", true, FixedRange.One, FixedRange.One, Options(activity.Actor).ToList());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
