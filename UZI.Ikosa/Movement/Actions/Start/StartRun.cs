using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Movement
{
    // TODO: lose dexterity, constitution checks

    [Serializable]
    public class StartRun : MoveSubAct
    {
        public StartRun(MovementBase movement)
            : base(movement, new ActionTime(TimeType.Total), true)
        {
        }

        private LinearMoveBudget _Linear = null;
        // TODO: flag to force rebalance if needed

        public override string Key => @"Movement.Start.Run";
        public override string DisplayName(CoreActor actor) => $@"Run ({Movement.Name})";
        public LinearMoveBudget LinearBudget => _Linear;

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets)
            => new CoreActivity(activity.Actor, new ContinueLinearMove(Movement), targets);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Run", activity.Actor, observer);

        public override double BaseCost
        {
            get
            {
                // run cost * base cost
                // NOTE: it may not be possible to run with a base.BaseCost above 1!
                return (Creature?.RunCost ?? 1d) * base.BaseCost;
            }
        }

        protected override IEnumerable<CoreStep> PreMoveSteps(MoveCostCheckStep step)
        {
            // only get linear budget when we're sure we can move
            var _vector = step.Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            _Linear = LinearMoveBudget.GetBudget(Budget);
            LinearBudget.Add(_vector);

            // must use double move
            yield return new ForceDoubleStep(step.Activity);
            yield break;
        }

        protected override IEnumerable<CoreStep> NormalMoveSteps(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            var _base = BaseCost;
            if (finalCost <= _base)
            {
                Creature.AddAdjunct(new RunPenalty());
                var _runLimit = Creature.Adjuncts.OfType<RunLimiter>().FirstOrDefault();
                if (_runLimit != null)
                {
                    _runLimit.AddRound();
                }
                else
                {
                    Creature.AddAdjunct(new RunLimiter());
                }
            }
            return BaseOnMoveCostCheck(step, finalCost);
        }

        public override void DoClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            base.DoClearStack(budget, activity);
            Creature.Adjuncts.OfType<RunPenalty>().FirstOrDefault()?.Eject();
        }

        protected override IEnumerable<CoreStep> PostMoveSteps(MoveCostCheckStep step)
        {
            yield return new TryPassThrough(step.Activity);
            yield break;
        }
    }
}
