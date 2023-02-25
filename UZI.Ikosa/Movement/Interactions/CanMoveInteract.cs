using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class CanMoveInteract : InteractData
    {
        public CanMoveInteract(CoreActor actor, ICoreObject candidate, bool? firstResult)
            : base(actor)
        {
            _First = firstResult;
            _Candidate = candidate;
        }

        #region state
        private bool? _First;
        private ICoreObject _Candidate;
        #endregion

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();

        private static CanMoveInteractHandler _Static = new CanMoveInteractHandler();

        public bool? FirstResult => _First;
        public ICoreObject Candidate => _Candidate;
    }
}
