using System;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Provides weapon proficiencies via the trait</summary>
    [Serializable]
    public class WrappedWeaponProficiencyTrait : TraitEffect
    {
        /// <summary>Provides weapon proficiencies via the trait</summary>
        public WrappedWeaponProficiencyTrait(ITraitSource traitSource, 
            IWeaponProficiency proficiency)
            : base(traitSource)
        {
            _Proficiency = proficiency;
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Proficiencies.Add(_Proficiency);
            }
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Proficiencies.Remove(_Proficiency);
            }

            base.OnDeactivate(source);
        }

        #region private data
        private IWeaponProficiency _Proficiency;
        #endregion

        public override object Clone()
        {
            return new WrappedWeaponProficiencyTrait(TraitSource, _Proficiency);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new WrappedWeaponProficiencyTrait(traitSource, _Proficiency);
        }
    }
}
