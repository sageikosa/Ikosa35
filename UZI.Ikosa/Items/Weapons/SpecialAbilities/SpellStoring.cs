using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class SpellStoring : WeaponSpecialAbility
    {
        public SpellStoring(object source)
            : base(source, 1, 0)
        {
        }

        public override object Clone()
        {
            return new SpellStoring(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"Can hold a spell for later release");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is SpellStoring;
    }
}
