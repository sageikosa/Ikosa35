using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>All budgets march to the beat of the time step, used when in time-tracking mode</summary>
    [Serializable]
    public class LocalTimeStep : CoreStep, ITurnTrackingStep
    {
        #region ctor()
        /// <summary>All budgets march to the beat of the time step, used when in time-tracking mode</summary>
        public LocalTimeStep(ITurnTrackingStep predecessor)
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

        public TimeTickablePrerequisite TimeTickablePrerequisite
            => GetPrerequisite<TimeTickablePrerequisite>();

        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (IsDispensingPrerequisites)
            {
                // time tickable (by schedule)
                if (!DispensedPrerequisites.OfType<TimeTickablePrerequisite>().Any())
                {
                    return new TimeTickablePrerequisite(Tracker);
                }
            }
            return null;
        }

        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() < 1;

        protected override bool OnDoStep()
        {
            Tracker?.CompleteStep(this);
            return true;
        }
    }
}
