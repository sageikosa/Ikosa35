using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ContinueLinearMove : StartCharge
    {
        public ContinueLinearMove(MovementBase movement)
            : base(movement, new ActionTime(TimeType.SubAction))
        {
        }

        public override string Key => @"Movement.ContinueLinear";
        public override string DisplayName(CoreActor actor) => $@"Continue Moving Linearly ({Movement.Name})";
        public override bool IsStackBase(CoreActivity activity) => false;
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity) => false;

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueLinearMove(Movement), targets);

        public override double BaseCost
            => (Budget.TopActivity?.Action as MovementAction)?.BaseCost ?? base.BaseCost;

        public override IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
        {
            // only get linear budget when we're sure we can move
            var _vector = step.Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            _Linear = LinearMoveBudget.GetBudget(Budget);
            LinearBudget.Add(_vector);

            var _base = this.BaseCost;
            if (finalCost > _base)
            {
                // TODO: remove running/chargings?
            }
            return BaseOnMoveCostCheck(step, finalCost);
        }

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new MovementAim(MovementTargets.Direction, @"Direction");
            yield break;
        }
        #endregion
    }
}