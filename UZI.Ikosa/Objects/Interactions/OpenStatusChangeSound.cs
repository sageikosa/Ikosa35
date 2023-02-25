using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class OpenStatusChangeSound : InteractData
    {
        public OpenStatusChangeSound(CoreActor actor, Func<Guid> idFactory, OpenStatus source, OpenStatus target, bool initial, bool blocked)
            : base(actor)
        {
            _Source = source;
            _Target = target;
            _Initial = initial;
            _Blocked = blocked;
            _IDFactory = idFactory;
        }

        #region state
        private OpenStatus _Source;
        private OpenStatus _Target;
        private bool _Initial;
        private bool _Blocked;
        private Func<Guid> _IDFactory;

        private static OpenStatusChangeSoundHandler _Handler = new OpenStatusChangeSoundHandler();
        #endregion

        public OpenStatus Source => _Source;
        public OpenStatus Target => _Target;
        public bool Initial => _Initial;
        public bool Blocked => _Blocked;
        public Func<Guid> IDFactory => _IDFactory;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Handler;
            yield break;
        }
    }
}
