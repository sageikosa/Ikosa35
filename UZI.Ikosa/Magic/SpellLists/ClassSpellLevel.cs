using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Magic.SpellLists
{
    public class ClassSpellLevel: Collection<ClassSpell>
    {
        public ClassSpellLevel(int level)
            : base()
        {
            Level = level;
        }
        public int Level { get; private set; }
    }
}
