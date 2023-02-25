using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class NeedsTurnTickStep : PreReqListStepBase, ITurnTrackingStep
    {
        public NeedsTurnTickStep(CoreStep predecessor, LocalTurnTracker tracker)
            : base(predecessor)
        {
            _Tracker = tracker;
            _PendingPreRequisites.Enqueue(new NeedsTurnTickPrerequisite(tracker));
        }

        private LocalTurnTracker _Tracker;

        public LocalTurnTracker Tracker => _Tracker;

        /// <summary>
        /// True if this step is intended to replace the current root step.
        /// Used by tracker processes to truncate the follower chain.
        /// </summary>
        public override bool IsNewRoot => true;

        protected override bool OnDoStep()
        {
            // complete
            Tracker?.CompleteStep(this);
            return true;
        }
    }
}
