using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class SpontaneousSpellSlotLevelModel : SpellSlotLevelModel<SpellSlot>
    {
        public SpontaneousSpellSlotLevelModel(SpellSlotLevel<SpellSlot> level, double time)
            :base(level)
        {
            _Slots = level.Slots.Select(_s => new SpontaneousSpellSlotModel(this, _s, time)).ToList<SpellSlotModel<SpellSlot>>();
        }
    }
}
