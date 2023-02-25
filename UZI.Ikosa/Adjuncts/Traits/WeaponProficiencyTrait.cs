using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class WeaponProficiencyTrait<Wpn> : TraitEffect, IWeaponProficiency
        where Wpn : IWeapon
    {
        public WeaponProficiencyTrait(ITraitSource traitSource)
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

        public override object Clone()
            => new WeaponProficiencyTrait<Wpn>(TraitSource);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new WeaponProficiencyTrait<Wpn>(traitSource);

        #region IWeaponProficiency Members

        public string Description
            => ItemBase.GetInfo(typeof(Wpn)).Name;

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => type == typeof(Wpn);

        #endregion
    }
}
