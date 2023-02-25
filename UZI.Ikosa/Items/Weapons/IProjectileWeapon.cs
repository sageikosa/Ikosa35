using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IProjectileWeapon : IRangedSource, IAttackSource, IDamageSource, IEnhancementTracker
    {
        /// <summary>Maximum strength bonus that can be applied to damage</summary>
        int MaxStrengthBonus { get; }
        IWeaponHead VirtualHead { get; }
        bool UsesStrengthDamage { get; }
        bool TakesStrengthDamagePenalty { get; }

        /// <summary>When in use, uses two hands, even if only 1 is slotted</summary>
        bool UsesTwoHands { get; }
    }
}
