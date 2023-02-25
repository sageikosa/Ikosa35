using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class DelayedTickStep : CoreStep, ITurnTrackingStep
    {
        public DelayedTickStep(CoreStep predecessor, LocalTurnTracker tracker)
            : base(predecessor)
        {
            _Tracker = tracker;
            _Budget = tracker.DelayedBudget;
        }

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
            => (_Budget != null) && (DispensedPrerequisites.Count() == 0);

        protected override bool OnDoStep()
        {
            if (_Budget != null)
            {
                var _inq = DispensedPrerequisites.OfType<ActionInquiryPrerequisite>().FirstOrDefault();
                if ((_inq != null) && (_inq.Acted ?? false))
                {
                    // since action occurred, we had a tick, and must end it
                    _Budget.EndTick();
                }
            }

            // tick tracking
            Tracker.CompleteStep(this);
            return true;
        }
    }
}
