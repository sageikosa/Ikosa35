using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MartialWeaponProficiencyTrait: TraitEffect, IWeaponProficiency
    {
        public MartialWeaponProficiencyTrait(ITraitSource traitSource)
            : base(traitSource)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Proficiencies.Add(this);
            }
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Proficiencies.Remove(this);
            }

            base.OnDeactivate(source);
        }

        #region IWeaponProficiency Members
        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // everything but exotic weapons (generally)
            return (typeof(IMartialWeapon).IsAssignableFrom(type));
        }
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            return (profType == WeaponProficiencyType.Martial);
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel) 
        {
            return IsProficientWith(weapon.ProficiencyType, powerLevel);
        }

        public string Description
        {
            get { return @"All martial weapons"; }
        }
        #endregion

        public override object Clone()
        {
            return new MartialWeaponProficiencyTrait(TraitSource);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new MartialWeaponProficiencyTrait(traitSource);
        }
    }
}
