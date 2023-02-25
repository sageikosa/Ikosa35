using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Implement this interface to handle attack.  WeaponHeads (for attacks), Grasp/Probe, AttackTriggerable</summary>
    public interface IAttackSource: IAdjunctSet, IActionSource
    {
        /// <summary>Attack source's enhancement delta to attack score</summary>
        DeltableQualifiedDelta AttackBonus { get; }
        
        /// <summary>Size of critical Range</summary>
        int CriticalRange { get; }
        
        /// <summary>Muliplicative factor for critcal range</summary>
        DeltableQualifiedDelta CriticalRangeFactor { get; }
        
        /// <summary>Damage multiplier for critical hits</summary>
        DeltableQualifiedDelta CriticalDamageFactor { get; }

        /// <summary>Gets prerequisites for an attack result</summary>
        IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack);

        /// <summary>Informs the attack source of the results of an attack.</summary>
        void AttackResult(AttackResultStep result, Interaction attack);

        /// <summary>
        /// True if the parameter represents the same attack source channel as this source.
        /// They can be the same weapon, the same weapon head, or ranged ammunition launched from a weapon.
        /// </summary>
        bool IsSourceChannel(IAttackSource source);
    }
}
