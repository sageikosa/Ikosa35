using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Default = 1.5d</summary>
    [Serializable]
    public class ObjectGrabbedCostHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectGrabbedCostData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is ObjectGrabbedCostData _ogcd)
            {
                workSet.Feedback.Add(new ValueFeedback<double>(this, 1.5d));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
