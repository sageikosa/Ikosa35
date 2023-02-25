using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// General interface to find information about a spell
    /// </summary>
    public interface ISpellInfo
    {
        // TODO: define a spell as it appears on various class lists, with normal action requirements
        // TODO: define a spell as it appears on a prepared action list
        string Name { get; }

        /// <summary>Textual description of the spell</summary>
        string Description { get; }

        MagicStyle MagicStyle { get; }

        /// <summary>Indicates whether a target is allowed spell resistance to ignore the spell</summary>
        bool AllowsSpellResistance { get; }

        /// <summary>Lists the descriptors for a spell definition.</summary>
        IEnumerable<Descriptor> Descriptors { get; }

        /// <summary>Spell level as defined, prepared, or cast</summary>
        int SpellLevel { get; }

        /// <summary>Casting time for the spell</summary>
        ActionTime CastingTime { get; }

        /// <summary>Indicates whether the caster can dismiss the spell at will</summary>
        bool IsDismissable { get; }

        /// <summary>Duration the spell stays in effect</summary>
        Duration Duration { get; }

        /// <summary>Range the spell target(s) may be from the caster</summary>
        Range Range { get; }

        IEnumerable<SpellComponent> DivineComponents { get; }
        IEnumerable<SpellComponent> ArcaneComponents { get; }

        // TARGETS, LOCATION, GEOMETRY?
        // SAVES (specific to effects)
    }
}
