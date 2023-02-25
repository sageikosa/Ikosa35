using System;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>If an interaction required some additional prerequisites</summary>
    [Serializable]
    public class RetryInteractionStep : InteractionPreReqStep
    {
        #region ctor()
        /// <summary>creates a step and automatically enqueues it in the predecessors following steps queue</summary>
        public RetryInteractionStep(CoreStep predecessor, string name, Interaction retry)
            : base(predecessor, retry)
        {
            _Name = name;
            _Interaction = retry;
        }

        public RetryInteractionStep(string name, Interaction retry)
            : base((CoreProcess)null, retry)
        {
            _Name = name;
            _Interaction = retry;
        }
        #endregion

        #region data
        private string _Name;
        #endregion

        public override string Name => _Name;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            StepPrerequisite _fail = FailingPrerequisite;
            if (_fail != null)
            {
                // TODO: inform that interaction retry failed?
            }
            else
            {
                // retry interaction (feedback has prerequisites attached, so they can be checked)
                _Interaction.Target.HandleInteraction(_Interaction);

                // if there are new unyielded prerequisites, create a new rety interaction step
                if (_Interaction.Feedback.OfType<PrerequisiteFeedback>().Any(_f => !_f.Yielded))
                {
                    // insert into the chain of next steps (the constructor automatically enqueues
                    var _retry = new RetryInteractionStep(this, Name, _Interaction);
                }

                // TODO: inform that interaction retry succeeded?
            }

            // done
            return true;
        }
        #endregion
    }
}
