using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class StartWithdraw : MoveSubAct
    {
        public StartWithdraw(MovementBase movement, ActionTime time)
            : base(movement, time, false)
        {
        }

        public override string Key => @"Movement.Start.Withdraw";
        public override string DisplayName(CoreActor actor) => $@"Withdraw ({Movement.Name})";

        public override IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
        {
            // must use double move
            yield return new ForceDoubleStep(step.Activity);
            foreach (var _step in BaseOnMoveCostCheck(step, finalCost))
                yield return _step;
            yield break;
        }

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            yield return new ForceDoubleStep(step.Activity);
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            return BaseOnMoveCostCheck(step, finalCost);
        }

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo($"Withdraw via {Movement.Name}", activity.Actor, observer);

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueMove(Movement), targets);
    }
}
