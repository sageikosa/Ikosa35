using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SpellSlotLevel<SlotClass>
        where SlotClass : SpellSlot, new()
    {
        public SpellSlotLevel(int level)
        {
            _Level = level;
            _Slots = new List<SlotClass>();
        }

        #region data
        private int _Level;
        internal List<SlotClass> _Slots;
        #endregion

        public int Level => _Level;
        public int SlotCount => _Slots.Count;
        public IEnumerable<SlotClass> Slots => _Slots.Select(_s => _s);
        public int IndexOf(SlotClass spellSlot) => _Slots.IndexOf(spellSlot);
        public SlotClass this[int index] => _Slots[index];
    }
}
