using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>Allows a set of steps to be emitted (with optional single prerequisites...)</summary>
    [Serializable]
    public class MultiNextStep : PreReqListStepBase
    {
        #region Construction
        /// <summary>nextSteps provided must already be bound to the activity</summary>
        public MultiNextStep(CoreStep predecessor, IEnumerable<CoreStep> nextSteps, IEnumerable<StepPrerequisite> preReqs)
            : base(predecessor)
        {
            _NextSteps = nextSteps;
            if (preReqs != null)
            {
                foreach (StepPrerequisite _pre in preReqs)
                {
                    _PendingPreRequisites.Enqueue(_pre);
                }
            }
        }

        /// <summary>nextSteps provided must already be bound to the activity</summary>
        public MultiNextStep(CoreActivity activity, IEnumerable<CoreStep> nextSteps, IEnumerable<StepPrerequisite> preReqs)
            : base(activity)
        {
            _NextSteps = nextSteps;
            if (preReqs != null)
            {
                foreach (StepPrerequisite _pre in preReqs)
                {
                    _PendingPreRequisites.Enqueue(_pre);
                }
            }
        }
        #endregion

        #region Private Data
        private IEnumerable<CoreStep> _NextSteps;
        #endregion

        protected override bool OnDoStep()
        {
            StepPrerequisite _fail = FailingPrerequisite;
            if (_fail != null)
            {
                // TODO: inform that interaction retry failed?
            }
            else
            {
                // TODO: inform that interaction retry succeeded?

                foreach (CoreStep _step in _NextSteps)
                {
                    ISplinterableStep _splinter = _step as ISplinterableStep;
                    if (_splinter != null)
                    {
                        _splinter.MasterStep = this;
                    }
                    AppendFollowing(_step);
                }
            }
            return true;
        }
    }
}
