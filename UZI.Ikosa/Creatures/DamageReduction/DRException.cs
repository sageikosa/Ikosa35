using System;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Default DamageReductionType is for nothing to overcome DR (ie., "DR/-")
    /// </summary>
    [Serializable]
    public class DRException
    {
        public DRException()
        {
        }

        public virtual string Name { get { return "-"; } }
        public virtual bool DoesWeaponIgnoreReduction(IWeaponHead weaponHead)
        {
            return false;
        }
    }
}
