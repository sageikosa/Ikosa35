using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class GraspHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            // this occurs after attack, so attack can ensure touch, no need to manipulate touch
            if (workSet.InteractData is GraspData _grasp)
            {
                workSet.Feedback.Add(new GraspFeedback(this)
                {
                    Information = new Info[]
                    {
                        new Info
                        {
                            Message= @"Contact"
                        }
                    }
                });
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GraspData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }

        #endregion
    }
}
