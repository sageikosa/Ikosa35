using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PowerActionTransitFeedback<PowerSrc> : PrerequisiteFeedback, ISuccessIndicatorFeedback
        where PowerSrc : IPowerActionSource
    {
        #region Construction
        public PowerActionTransitFeedback(Interaction workset, IInteract target, bool success)
            : base(workset)
        {
            _Success = success;
            _Message = string.Empty;
            _Target = target;
            _TransPreReq = new PowerActionTransitPrerequisite<PowerSrc>(@"Power.Transit", @"Power Transit", this);
        }

        public PowerActionTransitFeedback(Interaction workset, IInteract target, bool success, string message)
            : base(workset)
        {
            _Success = success;
            _Message = message;
            _Target = target;
            _TransPreReq = new PowerActionTransitPrerequisite<PowerSrc>(@"Power.Transit", @"Power Transit", this);
        }
        #endregion

        #region Private Data
        private PowerActionTransitPrerequisite<PowerSrc> _TransPreReq;
        private bool _Success;
        private string _Message;
        private IInteract _Target;
        #endregion

        public PowerActionTransitPrerequisite<PowerSrc> Prerequisite => _TransPreReq;

        /// <summary>Original Source InteractData cast as a PowerTransit</summary>
        public PowerActionTransit<PowerSrc> PowerTransit => (PowerActionTransit<PowerSrc>)WorkSet.InteractData;

        public Interaction WorkSet => Source as Interaction;

        public bool Success { get => _Success; set => _Success = value; }
        public string Message => _Message;
        public IInteract Target => _Target;

        public override IEnumerable<StepPrerequisite> Prerequisites
            => _TransPreReq.ToEnumerable();
    }
}
