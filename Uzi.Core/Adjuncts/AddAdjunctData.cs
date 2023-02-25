using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class AddAdjunctData : InteractData
    {
        public AddAdjunctData(CoreActor actor, Adjunct adjunct)
            : base(actor)
        {
            _Adjunct = adjunct;
        }

        private Adjunct _Adjunct;
        public Adjunct Adjunct { get { return _Adjunct; } }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Handler;
            yield break;
        }

        private AddAdjunctHandler _Handler = new AddAdjunctHandler();
    }
}
