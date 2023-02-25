using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ContinueMove : MoveSubAct
    {
        public ContinueMove(MovementBase movement)
            : base(movement, new ActionTime(TimeType.SubAction), true)
        {
        }

        public override string Key => @"Movement.Continue";
        public override string DisplayName(CoreActor actor) => $@"Continue Moving ({Movement.Name})";

        // NOTE: must leave these as false, since sometimes the time budget changes

        public override bool IsStackBase(CoreActivity activity) => false;
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity) => false;

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueMove(Movement), targets);

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
            => DiagonalOnMoveCostCheck(step, finalCost, atStart);

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }
    }
}
