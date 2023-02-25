using Uzi.Core;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    public interface IDamageSource
    {
        Alignment Alignment { get; }
        /// <summary>Damage source's enhancement delta to damage score</summary>
        DeltableQualifiedDelta DamageBonus { get; }

        /// <summary>Base damage source rollers (included in criticals)</summary>
        IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup);

        /// <summary>Extra damage rollers (not included in criticals)</summary>
        IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction workSet);

        /// <summary>Total damage rollers</summary>
        IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet);

        string EffectiveDamageRollString(Interaction workSet);
        bool IsMagicalDamage { get; }
        string MediumDamageRollString { get; set; }
        DamageType[] DamageTypes { get; }
    }
}
