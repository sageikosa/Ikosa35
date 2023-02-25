using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions.Action
{
    [Serializable]
    public class Purloin : InteractData
    {
        public Purloin(CoreActor actor, ActionTime actionTime)
            : base(actor)
        {
            _Time = actionTime;
        }

        #region state
        protected readonly ActionTime _Time;
        #endregion

        public ActionTime ActionTime => _Time;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return new PurloinHandler();
            yield break;
        }
    }
}
