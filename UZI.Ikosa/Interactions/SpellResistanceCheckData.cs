using System;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Used to calculate spell resistance check score</summary>
    [Serializable]
    public class SpellResistanceCheckData : InteractData
    {
        /// <summary>Used to calculate spell resistance check score</summary>
        public SpellResistanceCheckData(Creature critter, ISpellDef spellDef)
            : base(critter)
        {
            SpellDef = spellDef;
        }

        public ISpellDef SpellDef { get; private set; }
    }
}
