using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SilentStealthChoice : ActionBase
    {
        public SilentStealthChoice(IActionSource source)
            : base(source, new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
        }

        public override string Key => nameof(SilentStealthChoice);
        public override string DisplayName(CoreActor actor) => @"Stealth Mode Selection";
        public override bool IsMental => true;
        public override bool IsChoice => true;
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            if ((_Budget != null)
                && MovementRangeBudget.GetBudget(_Budget) is MovementRangeBudget _moveRange)
            {
                yield return new OptionAimOption
                {
                    Key = nameof(MovementRangeBudget.Stealth.None),
                    Name = @"None",
                    Description = @"No stealth (penalty: -10)",
                    IsCurrent = _moveRange.CurrentStealth == MovementRangeBudget.Stealth.None
                };

                // can still set, or is currently set to high
                if ((_moveRange.Double > 0) 
                    || (_moveRange.CurrentStealth == MovementRangeBudget.Stealth.High))
                {
                    yield return new OptionAimOption
                    {
                        Key = nameof(MovementRangeBudget.Stealth.High),
                        Name = @"High",
                        Description = @"Single move, half speed (penalty: 0)",
                        IsCurrent = _moveRange.CurrentStealth == MovementRangeBudget.Stealth.High
                    };
                }

                yield return new OptionAimOption
                {
                    Key = nameof(MovementRangeBudget.Stealth.Hasty),
                    Name = @"Hasty",
                    Description = @"Double move possible, half speed (penalty : 5)",
                    IsCurrent = _moveRange.CurrentStealth == MovementRangeBudget.Stealth.High
                };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"SilentStealth", @"Silent Stealth Mode", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((_Budget != null)
                && (MovementRangeBudget.GetBudget(_Budget) is MovementRangeBudget _moveRange)
                && (activity?.Targets[0] as OptionTarget is OptionTarget _target))
            {
                var _switch = _target.Option;
                switch (_switch.Key)
                {
                    case nameof(MovementRangeBudget.Stealth.High):
                        _moveRange.SetStealth(_Budget.Creature, MovementRangeBudget.Stealth.High);
                        break;

                    case nameof(MovementRangeBudget.Stealth.Hasty):
                        _moveRange.SetStealth(_Budget.Creature, MovementRangeBudget.Stealth.Hasty);
                        break;

                    case nameof(MovementRangeBudget.Stealth.None):
                    default:
                        _moveRange.SetStealth(_Budget.Creature, MovementRangeBudget.Stealth.None);
                        break;
                }

                // status step
                return activity.GetActivityResultNotifyStep($@"Stealth set to {_moveRange.CurrentStealth}");
            }

            // status step
            return activity.GetActivityResultNotifyStep(@"No current action budget to set");
        }
        #endregion
    }
}