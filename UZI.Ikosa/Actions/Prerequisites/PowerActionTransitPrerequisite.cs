using System;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Prerequisite that always IsReady, and can fail a process if the power transit failed</summary>
    [Serializable]
    public class PowerActionTransitPrerequisite<PowerSrc> : StepPrerequisite
        where PowerSrc : IPowerActionSource
    {
        public PowerActionTransitPrerequisite(string key, string name, PowerActionTransitFeedback<PowerSrc> feedback)
            : base(feedback.PowerTransit, feedback.WorkSet, key, name)
        {
            _Feedback = feedback;
        }

        private PowerActionTransitFeedback<PowerSrc> _Feedback;

        public PowerActionTransitFeedback<PowerSrc> Feedback => _Feedback;
        public override bool IsReady => true;
        public override bool FailsProcess => !Feedback.Success;
        public override bool IsSerial => false;

        /// <summary>Always null, as this prerequisite doesn't need to be fulfilled.  It starts ready</summary>
        public override CoreActor Fulfiller => null;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => null;

        public override void MergeFrom(PrerequisiteInfo info)
        {
        }
    }
}
