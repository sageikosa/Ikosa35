using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(5, typeof(CraftMagicArmsAndArmorFeat), typeof(CureLightWounds))]
    public class Merciful : WeaponSpecialAbility
    {
        public Merciful()
            : base(typeof(Merciful), 1,0)
        {
        }

        public override bool CanUseOnRanged { get { return true; } }

        public override object Clone()
        {
            return new Merciful();
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                // TODO: descriptions
                yield return new Description(
                    Name,
                    new string[]
                    {
                        @"Deals non-lethal damage",
                        @"Deals extra damage"
                    });
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Merciful;
    }
}
