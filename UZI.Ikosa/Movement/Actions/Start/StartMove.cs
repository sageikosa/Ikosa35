using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class StartMove : MoveSubAct
    {
        #region construction
        public StartMove(MovementBase movement)
            : base(movement, new ActionTime(TimeType.Brief), true)
        {
        }

        protected StartMove(MovementBase movement, ActionTime actionCost, bool provokesMelee)
            : base(movement, actionCost, provokesMelee)
        {
        }
        #endregion

        public override string Key => @"Movement.Start.Standard";
        public override string DisplayName(CoreActor actor) => $@"Start Moving ({Movement.Name})";

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueMove(Movement), targets);

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
            => DiagonalOnMoveCostCheck(step, finalCost, true);

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _mode in base.AimingMode(activity))
            {
                yield return _mode;
            }

            yield return new OptionAim(@"Double", @"Allow Double", true, FixedRange.One, FixedRange.One, DoubleOptions());
            yield break;
        }
    }
}
