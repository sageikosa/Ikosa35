using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Tumble : MoveSubAct
    {
        public Tumble(MovementBase movement, ActionTime actionTime)
            : base(movement, actionTime, true)
        {
        }

        public override string Key => @"Movement.Tumble";
        public override string DisplayName(CoreActor actor) => $@"Tumble ({Movement.Name})";

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new Tumble(Movement, new ActionTime(TimeType.SubAction)), targets);

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            return DiagonalOnMoveCostCheck(step, finalCost, atStart);
        }

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            // TODO: tumble workflow
            yield break;
        }

        // TODO: add tumble budget
    }
}
