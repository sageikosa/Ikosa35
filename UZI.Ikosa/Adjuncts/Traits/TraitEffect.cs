using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public abstract class TraitEffect : Adjunct
    {
        protected TraitEffect(ITraitSource traitSource) 
            : base(traitSource)
        {
        }

        public ITraitSource TraitSource 
            => Source as ITraitSource;

        public Creature Creature
            => Anchor as Creature;

        public abstract TraitEffect Clone(ITraitSource traitSource);
    }
}
