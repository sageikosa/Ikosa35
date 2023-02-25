using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IWeapon : ISlottedItem
    {
        WieldTemplate WieldTemplate { get; }
        WeaponProficiencyType ProficiencyType { get; }

        /// <summary>Indicates whether the weapon is currently active for combat</summary>
        bool IsActive { get; }

        /// <summary>Weapon strikes this weapon can generate</summary>
        IEnumerable<AttackActionBase> WeaponStrikes();

        /// <summary>true if weapon is currently treated as light</summary>
        bool IsLightWeapon { get; }

        /// <summary>true if weapon is sunderable</summary>
        bool IsSunderable { get; }

        /// <summary>True if standard use of this weapon provokes attacks from threatening opponents</summary>
        bool ProvokesMelee { get; }

        /// <summary>True is standard use of this weapon provokes attacks from targeted opponents</summary>
        bool ProvokesTarget { get; }

        /// <summary>True if it is possible to be proficient for this interaction 
        /// (some melee/ranged strikes are treated as non-proficient for certain weapons)
        /// </summary>
        bool IsProficiencySuitable(Interaction interact);

        /// <summary>WieldTemplate needed for wield for the creature</summary>
        WieldTemplate GetWieldTemplate();
    }
}
