using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class InteractHandlerTrait : TraitEffect
    {
        public InteractHandlerTrait(ITraitSource traitSource, IInteractHandler handler)
            : base(traitSource)
        {
            _Handler = handler;
        }

        #region data
        private IInteractHandler _Handler;
        #endregion

        public IInteractHandler Handler => _Handler;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.AddIInteractHandler(Handler);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.RemoveIInteractHandler(Handler);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new InteractHandlerTrait(TraitSource, Handler);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new InteractHandlerTrait(traitSource, Handler);
    }
}
