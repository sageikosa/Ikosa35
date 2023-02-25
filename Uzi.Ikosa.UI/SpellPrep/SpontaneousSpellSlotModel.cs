using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class SpontaneousSpellSlotModel : SpellSlotModel<SpellSlot>
    {
        public SpontaneousSpellSlotModel(SpontaneousSpellSlotLevelModel level, SpellSlot slot, double time)
            : base(level, slot, time)
        {
            AddMenuItems(_Menus);
        }

    }
}
