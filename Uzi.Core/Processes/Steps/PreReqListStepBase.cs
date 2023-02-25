using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>
    /// Base class step for handling prerequisites
    /// </summary>
    [Serializable]
    public abstract class PreReqListStepBase : CoreStep
    {
        #region Construction
        /// <summary>associates the activity, and enqueues the step within the predecessor's following steps queue</summary>
        protected PreReqListStepBase(CoreStep predecessor):
            base(predecessor)
        {
            _PendingPreRequisites = new Queue<StepPrerequisite>();
        }

        /// <summary>
        /// associates the process, typically used for root steps, reactive steps and completion steps
        /// process can be null, allowing the CoreProcess to set the root step internally
        /// </summary>
        protected PreReqListStepBase(CoreProcess process) :
            base(process)
        {
            _PendingPreRequisites = new Queue<StepPrerequisite>();
        }
        #endregion

        /// <summary>Enqueue any prerequisites which should be automatically handled</summary>
        protected Queue<StepPrerequisite> _PendingPreRequisites;

        /// <summary>True if the queue of pending prerequisites still has items.</summary>
        public override bool IsDispensingPrerequisites
        {
            get { return _PendingPreRequisites.Count > 0; }
        }

        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (_PendingPreRequisites.Count > 0)
            {
                return _PendingPreRequisites.Dequeue();
            }
            return null;
        }
    }
}
