using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DRDamageTypeException: DRException
    {
        public DRDamageTypeException(DamageType dmgType)
            : base()
        {
            _DamageType = dmgType;
        }

        protected DamageType _DamageType;
        public DamageType DamageType
        {
            get
            {
                return _DamageType;
            }
        }

        public override string Name
        {
            get
            {
                return _DamageType.ToString();
            }
        }

        public override bool DoesWeaponIgnoreReduction(IWeaponHead weaponHead)
        {
            foreach (DamageType _damageType in weaponHead.DamageTypes)
            {
                // if the bit-wise AND shows an intersection, then the weapon is good
                if ((_damageType & _DamageType) > 0)
                    return true;
            }
            return false;
        }
    }
}
