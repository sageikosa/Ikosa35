using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    /// <summary>blocks further use of movement</summary>
    [Serializable]
    public class CanStillMoveStep : CoreStep
    {
        /// <summary>blocks further use of movement</summary>
        /// <param name="activity"></param>
        /// <param name="budget"></param>
        public CanStillMoveStep(CoreActivity activity, MovementBudget budget)
            : base(activity)
        {
            Budget = budget;
        }

        public MovementBudget Budget { get; private set; }

        public CoreActivity Activity
            => Process as CoreActivity;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;

        public override string Name
            => @"Clear Can Still Move";

        protected override bool OnDoStep()
        {
            Budget.CanStillMove = false;
            EnqueueNotify(new RefreshNotify(true, false, false, false, false), Activity.Actor.ID);
            // TODO: can no longer move status...
            return true;
        }
    }
}
