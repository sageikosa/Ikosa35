using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class RapidShotChoice : ActionBase
    {
        public RapidShotChoice(IActionSource source)
            : base(source, new ActionTime(TimeType.Regular), new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
            _Budget = null;
        }

        public override string Key => @"RapidShot";
        public override string DisplayName(CoreActor actor) => @"Rapid Shot: -2 ATK, 1 extra ranged attack on full attack";
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public override bool IsMental => true;
        public override bool IsChoice => true;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (_Budget != null)
            {
                var _switch = (activity.Targets[0] as OptionTarget).Option;
                var _on = (_switch != null) && _switch.Key.Equals(@"On", StringComparison.OrdinalIgnoreCase);

                var _current = _Budget.Actor.Adjuncts.OfType<RapidShotBudgetFactory>()
                    .FirstOrDefault(_f => (_f.Source as Type) == typeof(RapidShotChoice));

                // do everything in-action (no steps, no prerequisites)
                if (_on && (_current == null))
                {
                    // factory to create rapid shot when full attack is made
                    _Budget.Actor.AddAdjunct(new RapidShotBudgetFactory(typeof(RapidShotChoice)));
                }
                else if (!_on && (_current != null))
                {
                    // remove rapid shot factory
                    _current.Eject();
                }
                _Budget.Choices[Key] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

                // status step
                return activity.GetActivityResultNotifyStep($@"Set to {_on}");
            }

            // status step
            return activity.GetActivityResultNotifyStep(@"No current action budget to set");
        }

        private IEnumerable<OptionAimOption> Options()
        {
            if ((_Budget != null) && _Budget.Actor.HasActiveAdjunct<RapidShotBudgetFactory>())
            {
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off" };
            }
            else
            {
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off", IsCurrent = true };
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On" };
            }
            yield break;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"RapidShot", @"Rapid Shot", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}