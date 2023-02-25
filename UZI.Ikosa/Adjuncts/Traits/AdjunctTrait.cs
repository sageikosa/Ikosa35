using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class AdjunctTrait : TraitEffect
    {
        public AdjunctTrait(ITraitSource traitSource, Adjunct adjunct)
            : base(traitSource)
        {
            _Adjunct = adjunct;
        }

        private Adjunct _Adjunct;
        public Adjunct Adjunct => _Adjunct;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Anchor.AddAdjunct(_Adjunct);
        }

        protected override void OnDeactivate(object source)
        {
            _Adjunct.Eject();
            base.OnDeactivate(source);
        }


        public override TraitEffect Clone(ITraitSource traitSource)
            => new AdjunctTrait(traitSource, _Adjunct.Clone() as Adjunct);

        public override object Clone()
            => new AdjunctTrait(TraitSource, _Adjunct.Clone() as Adjunct);
    }
}
