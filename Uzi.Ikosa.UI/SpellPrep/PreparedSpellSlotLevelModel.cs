using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class PreparedSpellSlotLevelModel : SpellSlotLevelModel<PreparedSpellSlot>
    {
        public PreparedSpellSlotLevelModel(IPreparedCasterClass caster, int slotIndex, SpellSlotLevel<PreparedSpellSlot> level, double time)
            : base(level)
        {
            var _spells = GetPreparableSpells(caster, slotIndex, level.Level).ToList();
            _Slots = level.Slots.Select(_s => new PreparedSpellSlotModel(this, caster, _spells, _s, time))
                .ToList<SpellSlotModel<PreparedSpellSlot>>();
        }

        private IEnumerable<ClassSpell> GetPreparableSpells(IPreparedCasterClass caster, int setIndex, int spellLevel)
        {
            return (from _prep in caster.PreparableSpells(setIndex)
                    where _prep.Level <= spellLevel
                    select _prep);
        }
    }
}
