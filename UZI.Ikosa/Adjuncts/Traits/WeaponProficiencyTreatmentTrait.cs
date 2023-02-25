using System;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class WeaponProficiencyTreatmentTrait : TraitEffect
    {
        public WeaponProficiencyTreatmentTrait(ITraitSource traitSource, IWeaponProficiencyTreatment treatment)
            : base(traitSource)
        {
            _Treatment = treatment;
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
                _critter.Proficiencies.Add(_Treatment);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
                _critter.Proficiencies.Remove(_Treatment);
            base.OnDeactivate(source);
        }

        #region private data
        private IWeaponProficiencyTreatment _Treatment;
        #endregion

        public override object Clone() 
            => new WeaponProficiencyTreatmentTrait(TraitSource, _Treatment);

        public override TraitEffect Clone(ITraitSource traitSource) 
            => new WeaponProficiencyTreatmentTrait(traitSource, _Treatment);
    }
}
