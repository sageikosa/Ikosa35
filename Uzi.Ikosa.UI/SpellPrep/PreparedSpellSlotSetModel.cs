using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class PreparedSpellSlotSetModel : SpellSlotSetModel<PreparedSpellSlot>
    {
        public PreparedSpellSlotSetModel(IPreparedCasterClass caster, int setIndex, string name, 
            SpellSlotSet<PreparedSpellSlot> spellSlotSet, double time)
            :base(setIndex, name, spellSlotSet)
        {
            _Levels = spellSlotSet.AllLevels.Select(_l => new PreparedSpellSlotLevelModel(caster, setIndex, _l, time))
                .ToList<SpellSlotLevelModel<PreparedSpellSlot>>();
        }
    }
}
