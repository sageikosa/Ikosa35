using System;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class NaturalWeaponTrait : TraitEffect
    {
        public NaturalWeaponTrait(ITraitSource traitSource, NaturalWeapon weapon)
            : base(traitSource)
        {
            _Weapon = weapon;
        }

        private NaturalWeapon _Weapon;

        public NaturalWeapon Weapon => _Weapon;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature).Body.NaturalWeapons.Add(Weapon);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature).Body.NaturalWeapons.Remove(Weapon);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new NaturalWeaponTrait(TraitSource, _Weapon.Clone());

        public override TraitEffect Clone(ITraitSource traitSource)
            => new NaturalWeaponTrait(traitSource, _Weapon.Clone());
    }
}
