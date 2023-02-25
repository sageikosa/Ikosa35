using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class HopUp : MoveSubAct
    {
        public HopUp(MovementBase movement, ActionTime time)
            : base(movement, time, true)
        {
            // track original time type (in case time demands change)
            _SubAct = time.ActionTimeType == Contracts.TimeType.SubAction;
        }

        private bool _SubAct;

        public override string Key => @"Movement.HopUp";
        public override string DisplayName(CoreActor actor) => $@"Hop Up ({Movement.Name})";
        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets) => null;

        public override bool IsStackBase(CoreActivity activity) => !_SubAct;
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity) => !_SubAct;

        public override void ProcessManagerInitialized(CoreActivity activity)
        {
            // suppress normal early validation
        }

        public bool Validate(CoreActivity activity)
        {
            _IsValid = Movement.IsValidActivity(activity, activity.MainObject);
            return _IsValid;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // hop-up check step does the hop-up check first...
            // ...and if it succeeds does the normal stuff for MovementAction.PerformActivity
            // cost is accounted for in LandMovement.OnMoveCostFactor
            return new HopUpCheckStep(activity, new Deltable(10));
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Hop-Up", activity.Actor, observer);

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
            yield return new TryPassThrough(step.Activity);
            yield break;
        }
    }
}
