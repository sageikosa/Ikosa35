using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Advancement
{
    public interface ICasterClass : IPowerClass
    {
        /// <summary>Arcane or divine caster</summary>
        MagicType MagicType { get; }

        /// <summary>Effective level for max spell level and spells/day</summary>
        IVolatileValue EffectiveLevel { get; }

        /// <summary>Alignment of the caster</summary>
        Alignment Alignment { get; }

        CastingAbilityBase SpellDifficultyAbility { get; }
        CastingAbilityBase BonusSpellAbility { get; }

        /// <summary>Base spell Difficulty (for 0 level spells).  QualifiedDeltas may be added for spell sources, or spell targets...</summary>
        IDeltable SpellDifficultyBase { get; }

        /// <summary>Specific casters may have additional spells, or have some spells for the class unavailable</summary>
        IEnumerable<ClassSpell> UsableSpells { get; }

        /// <summary>CLR type representing this class</summary>
        Type CasterClassType { get; }

        CasterClassInfo ToCasterClassInfo();

        /// <summary>True if can use descriptor</summary>
        bool CanUseDescriptor(Descriptor descriptor);
    }

    /// <summary>Used to present conditions for SpellDC</summary>
    public class SpellDCCondition: Interaction
    {
        /// <summary>Used to present conditions for SpellDC</summary>
        public SpellDCCondition(CoreActor actor, SpellSource spellSource, IInteract target):
            base(actor, spellSource, target, spellSource)
        {
        }

        public SpellSource SpellSource { get { return InteractData as SpellSource; } }
    }
}
