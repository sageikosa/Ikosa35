using System.Collections.Generic;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core;

namespace Uzi.Ikosa
{
    public interface IDamageReduction: ISourcedObject
    {
        /// <summary>True if the weapon does ignore this reduction</summary>
        bool WeaponIgnoresReduction(IWeaponHead weaponHead);

        int Amount { get; }
        string Name { get; }

        /// <summary>Record use of reduction in case source gets consumed</summary>
        void HasReduced(int amount);
    }
}
