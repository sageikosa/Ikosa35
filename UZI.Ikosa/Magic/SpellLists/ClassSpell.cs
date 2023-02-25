using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.SpellLists
{
    [Serializable]
    public class ClassSpell
    {
        public ClassSpell(int level, SpellDef spellDef)
        {
            _Lvl = level;
            _Def = spellDef;
        }

        #region data
        private readonly int _Lvl;
        private readonly SpellDef _Def;
        #endregion

        public int Level => _Lvl;
        public SpellDef SpellDef => _Def;

        public virtual ClassSpellInfo ToClassSpellInfo()
            => new ClassSpellInfo
            {
                Level = Level,
                SpellDef = SpellDef.GetSpellDefInfo(),
                Message = SpellDef.DisplayName
            };
    }
}
