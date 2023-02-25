using System.Collections.Generic;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Advancement
{
    public interface ISpontaneousCaster : ISlottedCasterClass<SpellSlot>
    {
        IEnumerable<KeyValuePair<int, int>> KnownSpellsPerLevel(int level);
        IEnumerable<KeyValuePair<int, int>> KnownSpellNumbers { get; }
        KnownSpellSet KnownSpells { get; }
        SpellSlot GetAvailableSlot(int level);
    }
}
