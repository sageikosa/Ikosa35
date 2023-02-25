using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Waits for the actor (if any) to act or delay, then tick forward and get next step from tracker
    /// </summary>
    [Serializable]
    public class LocalTickStep : CoreStep, ITurnTrackingStep
    {
        #region ctor()
        public LocalTickStep(CoreStep predecessor, LocalTurnTracker tracker)
            : base(predecessor)
        {
            _Tracker = tracker;
            _Budget = Tracker.LeadBudgets.FirstOrDefault();
        }
        #endregion

        #region data
        private LocalActionBudget _Budget;
        private LocalTurnTracker _Tracker;
        #endregion

        public LocalTurnTracker Tracker => _Tracker;
        public LocalActionBudget Budget => _Budget;

        /// <summary>
        /// True if this step is intended to replace the current root step.
        /// Used by tracker processes to truncate the follower chain.
        /// </summary>
        public override bool IsNewRoot => true;

        /// <summary>Gets a single ActionInquirtPrerequisite if the tick has a budget using it</summary>
        protected override StepPrerequisite OnNextPrerequisite()
            => (IsDispensingPrerequisites)
            ? new ActionInquiryPrerequisite(this, _Budget)
            : null;

        /// <summary>True if there is a budget for this tick, and no prerequisites have been dispensed yet</summary>
        public override bool IsDispensingPrerequisites
            => (_Budget != null) && !DispensedPrerequisites.Any();

        protected override bool OnDoStep()
        {
            // find all budgets that do something with the end of tick
            _Budget?.EndTick();

            // tick tracking
            Tracker.CompleteStep(this);
            return true;
        }
    }
}
