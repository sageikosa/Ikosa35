using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Magic.SpellLists;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class PrepareSpellModel
    {
        public ClassSpell ClassSpell { get; set; }
        public PreparedSpellSlotModel SlotModel { get; set; }
    }
}
