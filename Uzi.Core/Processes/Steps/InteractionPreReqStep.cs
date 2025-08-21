using System;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// Step that manages prerequisites passed from interactions
    /// </summary>
    [Serializable]
    public abstract class InteractionPreReqStep : PreReqListStepBase
    {
        #region ctor()
        protected InteractionPreReqStep(CoreStep predecessor, Interaction interaction)
            : this(predecessor.Process, interaction)
        {
            predecessor.AppendFollowing(this);
        }

        protected InteractionPreReqStep(CoreProcess process, Interaction interaction)
            : base(process)
        {
            _Interaction = interaction;

            // feedback that requires prerequisites and hasn't already yielded them
            if (_Interaction != null)
            {
                foreach (var _preFeed in from _preF in interaction.Feedback.OfType<PrerequisiteFeedback>().ToList()
                                         where !_preF.Yielded
                                         select _preF)
                {
                    // get prerequisites
                    foreach (var _stepPre in _preFeed.Prerequisites)
                    {
                        _PendingPreRequisites.Enqueue(_stepPre);
                    }

                    // indicate the prerequisites have been yielded
                    _preFeed.Yielded = true;
                }
            }
        }
        #endregion

        #region data
        protected Interaction _Interaction;
        #endregion
    }
}
