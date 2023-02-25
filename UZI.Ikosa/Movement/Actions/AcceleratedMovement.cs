using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class AcceleratedMovement<MoveType> : ActionBase
        where MoveType : MovementBase, IAcceleratedMovement
    {
        public AcceleratedMovement(MoveType movement)
            : base(movement, new ActionTime(TimeType.Free), false, false, @"200")
        {
        }

        public MoveType Movement => Source as MoveType;
        public override string Key => @"Movement.Accelerated";
        public override string DisplayName(CoreActor actor) => $@"Set Accelerated Movement ({Movement.Name})";
        public override bool IsStackBase(CoreActivity activity) => false;
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity) => false;
        public override bool IsMental => true;
        public override bool IsChoice => true;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (_Budget != null)
            {
                var _switch = activity.GetFirstTarget<OptionTarget>(@"Accelerated")?.Option;
                var _on = (_switch != null) && _switch.Key.Equals(@"On", StringComparison.OrdinalIgnoreCase);

                if (_on != Movement.IsAccelerated)
                {
                    // turn accelerated on or off
                    Movement.IsAccelerated = _on;

                    // switching: expire climbing check
                    foreach (var _climbing in Movement.CoreObject.Adjuncts.OfType<Climbing>())
                    {
                        _climbing.IsCheckExpired = true;
                    }

                    // status step
                    return activity.GetActivityResultNotifyStep($@"Set to {_on}");
                }

                // status step
                return activity.GetActivityResultNotifyStep(@"Already set to this value");
            }
            // status step
            return activity.GetActivityResultNotifyStep(@"No current action budget to set");
        }
        #endregion

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            if (!Movement.IsAccelerated)
            {
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off", IsCurrent = true };
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On" };
            }
            else
            {
                yield return new OptionAimOption { Key = @"On", Name = @"On", Description = @"On", IsCurrent = true };
                yield return new OptionAimOption { Key = @"Off", Name = @"Off", Description = @"Off" };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Accelerated", string.Format(@"Accelerated Movement ({0})", Movement.Name), true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
