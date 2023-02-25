using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Magic.SpellLists
{
    /// <summary>List of all spell levels for a casting class</summary>
    [Serializable]
    public class ClassSpellList: Dictionary<int, ClassSpellLevel>
    {
        public ClassSpellList() :
            base()
        {
        }
    }
}
