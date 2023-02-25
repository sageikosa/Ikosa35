using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Represents one of each of several possible operating modes of a spell
    /// </summary>
    public interface ISpellMode : ICapabilityRoot
    {
        string DisplayName { get; }
        string Description { get; }

        IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode);
        bool AllowsSpellResistance { get; }

        /// <summary>If allows spell resistance, this indicates whether the spell is harmless.  Also used to determine need for turn-tracker.</summary>
        bool IsHarmless { get; }

        /// <summary>
        /// Attempt to deliver spell to all targets
        /// </summary>
        /// <param name="source"></param>
        /// <param name="spellMode">Allows a wrapper mode (such as metamagic)</param>
        /// <param name="originator"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        void ActivateSpell(PowerActivationStep<SpellSource> activation);

        /// <summary>Apply a spell that was successfully delivered</summary>
        void ApplySpell(PowerApplyStep<SpellSource> apply);
    }
}
