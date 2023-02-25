using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    public interface ISecondaryAttackResult
    {
        object AttackResultSource { get; }

        /// <summary>Gets prerequisites for an attack result</summary>
        IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack);

        /// <summary>Informs the weapon of the results of an attack</summary>
        void AttackResult(StepInteraction deliverDamageInteraction);

        bool PoweredUp { get; set; }

        /// <summary>
        /// Determines whether the secondary attack result can be applied given the damage applied.  
        /// Some secondary results require real weapon damage (poison, stunning, disease vector), 
        /// whereas some might require only energy (catching fire, shatter chance, energy drain).
        /// </summary>
        bool IsDamageSufficient(StepInteraction final);
    }
}
