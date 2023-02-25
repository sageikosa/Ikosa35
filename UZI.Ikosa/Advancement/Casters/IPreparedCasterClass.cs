using System.Collections.Generic;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;

namespace Uzi.Ikosa.Advancement
{
    public interface IPreparedCasterClass : ISlottedCasterClass<PreparedSpellSlot>
    {
        IEnumerable<ClassSpell> PreparableSpells(int setIndex);
    }
}
