using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class MoveSubAct : MovementAction
    {
        protected MoveSubAct(MovementBase movement, ActionTime time, bool provokesMelee = true)
            : base(movement, time, provokesMelee)
        {
        }

        /// <summary>Actions can stack if this starts a movement action</summary>
        public override bool IsStackBase(CoreActivity activity)
            => TimeCost.ActionTimeType != TimeType.SubAction;

        /// <summary>Pops actions from stack if this starts a movement action</summary>
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => TimeCost.ActionTimeType != TimeType.SubAction;

        #region public override IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
        public override IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
        {
            var _any = false;

            // pre move steps
            foreach (var _step in PreMoveSteps(step))
            {
                yield return _step;
            }

            // all normal steps
            foreach (var _step in NormalMoveSteps(step, finalCost, false))
            {
                _any = true;
                yield return _step;
            }

            // post steps
            if (_any)
            {
                foreach (var _step in PostMoveSteps(step))
                    yield return _step;
            }
        }
        #endregion

        protected abstract IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step);
        protected abstract IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart);
        protected abstract IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step);

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _aim in base.AimingMode(activity))
            {
                yield return _aim;
            }
            if (TimeCost.ActionTimeType == TimeType.SubAction)
                yield return new OptionAim(@"Double", @"Allow Double", true, FixedRange.One, FixedRange.One, DoubleOptions());
            yield break;
        }
        #endregion
    }
}
