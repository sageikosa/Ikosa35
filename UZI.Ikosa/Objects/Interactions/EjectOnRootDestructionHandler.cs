using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class EjectOnRootDestructionHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is RootDestruction _destroy)
                && (workSet.Target is IObjectBase _obj)) 
            {
                _obj.UnPath();
                var _contained = _destroy.Root.ContainedWithin();
                if (_contained != null)
                {
                    var _container = _obj.FindAcceptableContainer(_contained.Container);
                    if (_container?.CanHold(_obj) ?? false)
                    {
                        _container.Add(_obj);
                    }
                    else
                    {
                        Drop.DoDropEject(_contained.Container, _obj);
                    }
                }
                else
                {
                    Drop.DoDropEject(_destroy.Root, _obj);
                }
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(RootDestruction);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
