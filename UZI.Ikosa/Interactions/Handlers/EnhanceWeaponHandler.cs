using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    // TODO: enforce enhancement rules...
    [Serializable]
    public class EnhanceWeaponHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            throw new NotImplementedException();
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            throw new NotImplementedException();
        }
    }
}
