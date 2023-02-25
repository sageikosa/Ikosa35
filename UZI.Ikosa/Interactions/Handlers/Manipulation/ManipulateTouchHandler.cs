using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ManipulateTouchHandler : ManipulationHandlerBase, IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is ManipulateTouch _manipulate)
            {
                if ((workSet.Target is ICoreObject _obj)
                    && workSet.Actor is Creature _critter)
                {
                    var _objEthereal = _obj.PathHasActiveAdjunct<EtherealState>();
                    if ((_objEthereal == _critter.PathHasActiveAdjunct<EtherealState>())
                        || CanSubstantiallyInteract(_critter, _obj))
                    {
                        workSet.Feedback.Add(new ValueFeedback<bool>(this, true));
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
            => typeof(ManipulateTouch).ToEnumerable();

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
