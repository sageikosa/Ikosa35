using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class SpontaneousSpellSlotSetModel : SpellSlotSetModel<SpellSlot>
    {
        public SpontaneousSpellSlotSetModel(int setIndex, string name,
            SpellSlotSet<SpellSlot> spellSlotSet, double time)
            : base(setIndex, name, spellSlotSet)
        {
            _Levels = spellSlotSet.AllLevels.Select(_l => new SpontaneousSpellSlotLevelModel(_l, time))
                .ToList<SpellSlotLevelModel<SpellSlot>>();
        }
    }
}
