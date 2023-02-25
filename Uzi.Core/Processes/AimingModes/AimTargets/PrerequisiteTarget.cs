using System;

namespace Uzi.Core
{
    /// <summary>
    /// Used to pass a step prerequisite as a target to a spell effect
    /// </summary>
    [Serializable]
    public class PrerequisiteTarget : AimTarget
    {
        public PrerequisiteTarget(StepPrerequisite preReq)
            : base(preReq.BindKey, null)
        {
            _PreRequisite = preReq;
        }

        private StepPrerequisite _PreRequisite;
        public StepPrerequisite PreRequisite { get { return _PreRequisite; } }
    }
}
