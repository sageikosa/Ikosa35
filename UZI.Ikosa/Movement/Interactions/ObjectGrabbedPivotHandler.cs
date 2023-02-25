using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ObjectGrabbedPivotHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectGrabbedPivotData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is ObjectGrabbedPivotData _obpd)
                workSet.Feedback.Add(new ValueFeedback<int>(this, 1));
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
