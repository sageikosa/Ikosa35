using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class RemoveAdjunctData : InteractData
    {
        public RemoveAdjunctData(CoreActor actor, Adjunct adjunct)
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

        private RemoveAdjunctHandler _Handler = new RemoveAdjunctHandler();

        public bool DidRemove(Interaction workset)
            => workset.Feedback.OfType<ValueFeedback<bool>>().Any(_r => _r.Value);

    }
}
