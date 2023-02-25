using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Magic.Spells
{

    [Serializable]
    public class SpiritualWeaponTarget : GroupMemberAdjunct
    {
        public SpiritualWeaponTarget(SpellSource source, SpiritualWeaponGroup group)
            : base(source, group)
        {
        }

        public SpiritualWeaponGroup SpiritualWeaponGroup => Group as SpiritualWeaponGroup;
        public SpellSource SpellSource => Source as SpellSource;

        public override object Clone()
            => new SpiritualWeaponTarget(SpellSource, SpiritualWeaponGroup);
    }
}
