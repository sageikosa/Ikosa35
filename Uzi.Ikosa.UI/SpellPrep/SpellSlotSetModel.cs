using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public abstract class SpellSlotSetModel<SlotType>
        where SlotType : SpellSlot, new()
    {
        protected SpellSlotSetModel(int setIndex, string name, SpellSlotSet<SlotType> spellSlotSet)
        {
            _SetIndex = setIndex;
            _Name = name;
            _Set = spellSlotSet;
        }

        #region private data
        protected List<SpellSlotLevelModel<SlotType>> _Levels;
        private SpellSlotSet<SlotType> _Set;
        private string _Name;
        private int _SetIndex;
        #endregion

        public int SetIndex => _SetIndex;
        public string Name => _Name;
        public SpellSlotSet<SlotType> SpellSlotSet => _Set;
        public IEnumerable<SpellSlotLevelModel<SlotType>> Levels => _Levels.Select(_l => _l).ToList();
    }
}
