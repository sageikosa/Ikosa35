using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic.SpellLists;

namespace Uzi.Ikosa.Advancement
{
    public interface ISlottedCasterBaseClass : ICasterClass
    {
        /// <summary>Caster must rest before recharge</summary>
        bool MustRestToRecharge { get; }

        /// <summary>Count of distinct slot sets</summary>
        int SlotSetCount { get; }

        /// <summary>list of Spells per day per level by slot set, when class is at a specific level</summary>
        IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDayAtLevel(int setIndex, int level);

        /// <summary>list of Spells per day per level by slot set, for current class level</summary>
        IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDay(int setIndex);

        /// <summary>maximum spell level when class is at a specific level</summary>
        int MaximumSpellLevelAtLevel(int level);

        /// <summary>For spontaneous casters, typically the known spells.  For prepared caster, the spells they can prepare.</summary>
        IEnumerable<ClassSpell> SlottableSpells(int setIndex);

        string SpellSlotsName(int setIndex);
    }
}
