using System;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class WrappedArmorProficiencyTrait : TraitEffect
    {
        /// <summary>Provides armor proficiencies via the trait</summary>
        public WrappedArmorProficiencyTrait(ITraitSource traitSource, IArmorProficiency proficiency)
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
        private IArmorProficiency _Proficiency;
        #endregion

        public override object Clone()
        {
            return new WrappedArmorProficiencyTrait(TraitSource, _Proficiency);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new WrappedArmorProficiencyTrait(traitSource, _Proficiency);
        }
    }
}
