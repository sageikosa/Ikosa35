using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class StartCharge : MoveSubAct
    {
        public StartCharge(MovementBase movement, ActionTime timeCost)
            : base(movement, timeCost, true)
        {
        }

        #region data
        protected LinearMoveBudget _Linear = null;
        // TODO: flag to force rebalance if needed
        #endregion

        public override string Key => @"Movement.Start.Charge";
        public override string DisplayName(CoreActor actor) => $@"Charge ({Movement.Name})";
        public override bool CombatList => true;
        public LinearMoveBudget LinearBudget => _Linear;

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueLinearMove(Movement), targets);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Charge", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

        /// <summary>Overrides default IsHarmless setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless 
            => false;

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            // only get linear budget when we're sure we can move
            var _vector = step.Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            _Linear = LinearMoveBudget.GetBudget(Budget);
            LinearBudget.Add(_vector);

            // register activity upon successful attempted start of action
            yield return new ForceDoubleStep(step.Activity);
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            var _base = BaseCost;
            if (finalCost <= _base)
            {
                // TODO: charging adjunct?
            }
            return BaseOnMoveCostCheck(step, finalCost);
        }

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }

        public override void DoClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            base.DoClearStack(budget, activity);
            // TODO: clear charge adjunct
        }

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new FarRange(), new CreatureTargetType());
            foreach (var _aim in base.AimingMode(activity))
                yield return _aim;
            yield break;
        }
        #endregion
    }
}
