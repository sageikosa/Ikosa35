using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Advancement
{
    public interface ISlottedCasterClass<SlotClass> : ISlottedCasterBaseClass
        where SlotClass : SpellSlot, new()
    {
        /// <summary>get a spell slot set</summary>
        SpellSlotSet<SlotClass> GetSpellSlotSets(int setIndex);

        /// <summary>tuple[0]=setIndex, tuple[1]=setName, tuple[2]=SpellSlotSet</summary>
        IEnumerable<(int SetIndex, string SetName, SpellSlotSet<SlotClass> SlotSet)> AllSpellSlotSets { get; }

        IEnumerable<SlotClass> AllSpellSlots { get; }
    }
}
