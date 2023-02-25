using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class DestroyOnRootDestructionHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is RootDestruction _destroy)
                && (workSet.Target is IObjectBase _obj)) 
            {
                _obj.DoDestruction();
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(RootDestruction);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler is EjectOnRootDestructionHandler;
    }
}
