using System.Collections.Generic;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Advancement
{
    public interface IKnowSpells
    {
        IEnumerable<KnownSpell> KnownSpells { get; }
        // TODO: add, remove, and replace spells, and maintain a log...?
    }
}
