using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalStartOfTickStep : CoreStep, ITurnTrackingStep
    {
        #region ctor()
        /// <summary>All budgets march to the beat of the time step, used when in time-tracking mode</summary>
        public LocalStartOfTickStep(LocalTurnTracker tracker)
            : base((CoreProcess)null)
        {
            _Tracker = tracker;
        }

        /// <summary>All budgets march to the beat of the time step, used when in time-tracking mode</summary>
        public LocalStartOfTickStep(ITurnTrackingStep predecessor)
            : base(predecessor as CoreStep)
        {
            _Tracker = predecessor.Tracker;
        }
        #endregion

        #region data
        private LocalTurnTracker _Tracker;
        #endregion

        public LocalTurnTracker Tracker => _Tracker;

        /// <summary>
        /// True if this step is intended to replace the current root step.
        /// Used by tracker processes to truncate the follower chain.
        /// </summary>
        public override bool IsNewRoot => true;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            Tracker?.StartOfTick();
            return true;
        }
    }
}
