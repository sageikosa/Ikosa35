using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class SpellSlotSetTargeting : ViewModelBase
    {
        public SpellSlotSetTargeting(PrepareSpellSlotsTargeting prepareSpellSlots, SpellSlotSetInfo spellSlotSet)
        {
            _PrepareSpellSlots = prepareSpellSlots;
            _SlotSet = spellSlotSet;
            _Levels = spellSlotSet.SlotLevels.Select(_l => new SpellSlotLevelTargeting(this, _l)).ToList();
        }

        #region data
        private readonly PrepareSpellSlotsTargeting _PrepareSpellSlots;
        private readonly SpellSlotSetInfo _SlotSet;
        private readonly List<SpellSlotLevelTargeting> _Levels;
        #endregion

        public PrepareSpellSlotsTargeting PrepareSpellSlots => _PrepareSpellSlots;
        public SpellSlotSetInfo SpellSlotSetInfo => _SlotSet;
        public List<SpellSlotLevelTargeting> SlotLevels => _Levels;
    }
}
