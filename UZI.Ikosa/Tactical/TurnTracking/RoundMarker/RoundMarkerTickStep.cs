using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class RoundMarkerTickStep : PreReqListStepBase, ITurnTrackingStep
    {
        #region ctor()
        public RoundMarkerTickStep(CoreStep predecessor, LocalTurnTracker tracker)
            : base(predecessor)
        {
            _Tracker = tracker;
            Tracker.StartOfTick();
            _PendingPreRequisites.Enqueue(new RoundMarkerCompletePrerequisite(Tracker));
        }
        #endregion

        #region data
        private LocalTurnTracker _Tracker;
        #endregion

        public LocalTurnTracker Tracker => _Tracker;
        public IList<LocalActionBudget> Budgets => Tracker.LeadBudgets.ToList();

        /// <summary>
        /// True if this step is intended to replace the current root step.
        /// Used by tracker processes to truncate the follower chain.
        /// </summary>
        public override bool IsNewRoot => true;

        public RoundMarkerCompletePrerequisite RoundMarkerCompletePrerequisite
            => GetPrerequisite<RoundMarkerCompletePrerequisite>();

        protected override bool OnDoStep()
        {
            // find all budgets that do something with the end of tick
            foreach (var _budget in Budgets)
            {
                _budget?.EndTick();
            }

            // tick tracking
            Tracker?.CompleteStep(this);
            return true;
        }
    }
}
