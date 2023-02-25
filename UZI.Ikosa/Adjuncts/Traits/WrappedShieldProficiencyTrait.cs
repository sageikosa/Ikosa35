using System;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class WrappedShieldProficiencyTrait : TraitEffect
    {
        /// <summary>Provides shield proficiencies via the trait</summary>
        public WrappedShieldProficiencyTrait(ITraitSource traitSource, IShieldProficiency proficiency)
            : base(traitSource)
        {
            _Proficiency = proficiency;
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Proficiencies.Add(_Proficiency);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Proficiencies.Remove(_Proficiency);
            base.OnDeactivate(source);
        }

        #region private data
        private IShieldProficiency _Proficiency;
        #endregion

        public override object Clone()
        {
            return new WrappedShieldProficiencyTrait(TraitSource, _Proficiency);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new WrappedShieldProficiencyTrait(traitSource, _Proficiency);
        }
    }
}
