using System;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ExtraordinaryTrait : TraitBase
    {
        public ExtraordinaryTrait(ITraitSource traitSource, string name, string benefit, TraitCategory category, TraitEffect trait)
            : base(traitSource, name, benefit, category, trait)
        {
        }

        public override string TraitNature
            => @"Extraordinary";

        public override object Clone()
            => new ExtraordinaryTrait(TraitSource, Name, Benefit, TraitCategory, Trait.Clone(TraitSource));

        public override TraitBase Clone(ITraitSource traitSource)
            => new ExtraordinaryTrait(traitSource, Name, Benefit, TraitCategory, Trait.Clone(traitSource));
    }
}
