using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SimpleWeaponProficiencyTrait : TraitEffect, IWeaponProficiency
    {
        public SimpleWeaponProficiencyTrait(ITraitSource traitSource)
            : base(traitSource)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Proficiencies.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Proficiencies.Remove(this);
            base.OnDeactivate(source);
        }

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType == WeaponProficiencyType.Simple);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // true if the normal type not martial and not exotic
            return ((!typeof(IMartialWeapon).IsAssignableFrom(type))
                && (!typeof(IExoticWeapon).IsAssignableFrom(type)));
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        public string Description
            => @"All Simple Weapons";

        #endregion

        public override object Clone()
            => new SimpleWeaponProficiencyTrait(TraitSource);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new SimpleWeaponProficiencyTrait(traitSource);
    }
}
