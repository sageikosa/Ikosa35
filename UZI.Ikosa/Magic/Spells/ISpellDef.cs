using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Spell Definition</summary>
    public interface ISpellDef : IMagicPowerActionDef
    {
        IEnumerable<SpellComponent> DivineComponents { get; }
        IEnumerable<SpellComponent> ArcaneComponents { get; }

        /// <summary>Spell mode includes aiming and save information</summary>
        IEnumerable<ISpellMode> SpellModes { get; }

        /// <summary>Unmodified SpellDef at the heart of the magic</summary>
        SpellDef SeedSpellDef { get; }

        /// <summary>If presented as an Arcane spell, limited to charisma-based casting classes (at best)</summary>
        bool ArcaneCharisma { get; }

        /// <summary>Used when a spell can be treated as if it were another spell</summary>
        IEnumerable<Type> SimilarSpells { get; }
    }
}
