using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ConstrictTrait : TraitEffect
    {
        public ConstrictTrait(ITraitSource traitSource)
            : base(traitSource)
        {
        }

        // cloning
        public override TraitEffect Clone(ITraitSource traitSource)
            => new ConstrictTrait(traitSource);

        public override object Clone()
            => new ConstrictTrait(TraitSource);
    }
}
