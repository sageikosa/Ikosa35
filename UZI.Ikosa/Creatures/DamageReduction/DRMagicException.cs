using System;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DRMagicException : DRException
    {
        public DRMagicException()
            : base()
        {
        }

        public override string Name { get { return @"Magic"; } }
        public override bool DoesWeaponIgnoreReduction(Items.Weapons.IWeaponHead weaponHead)
        {
            return weaponHead.IsMagicalDamage;
        }
    }
}
