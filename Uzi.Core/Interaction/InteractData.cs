using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public abstract class InteractData
    {
        // NOTE: actor can be null
        protected InteractData(CoreActor actor)
        {
            _Alters = new AlterationSet(actor);
        }

        public AlterationSet Alterations => _Alters;
        private readonly AlterationSet _Alters;

        public virtual IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield break;
        }

        public virtual Type ProcessType => GetType();
    }
}
