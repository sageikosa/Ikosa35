using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>Handle an interaction as a step, allowing other steps to pre-empt</summary>
    [Serializable]
    public class HandleInteractionStep : CoreStep
    {
        public HandleInteractionStep(CoreProcess process, IInteract interactor, Interaction interaction)
            : base(process)
        {
            _Interactor = interactor;
            _Interaction = interaction;
        }

        #region state
        private IInteract _Interactor;
        private Interaction _Interaction;
        #endregion

        public override bool IsDispensingPrerequisites => false;

        public IInteract Interactor => _Interactor;
        public Interaction Interaction => _Interaction;

        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            Interactor?.HandleInteraction(Interaction);
            // TODO: consider feedback that requires subsequent processing: prerequisite/requireSave/requireSuccessCheck/info...
            return true;
        }
    }
}
