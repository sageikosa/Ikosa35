using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class FallStop : InteractData
    {
        public FallStop(Locator locator, List<string> messages)
            : base(null)
        {
            _Locator = locator;
            _Messages = messages;
        }

        #region data
        private Locator _Locator;
        private List<string> _Messages;
        #endregion

        public Locator Locator => _Locator;
        public List<string> Messages => _Messages;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return new FallStopDefault();
            yield break;
        }
    }

    public class FallStopDefault : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(FallStop);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.Target is Creature)
            {
                (new CreatureFallStopHandler()).HandleInteraction(workSet);
            }
            else if (workSet.Target is Trove)
            {
                (new TroveFallStopHandler()).HandleInteraction(workSet);
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
