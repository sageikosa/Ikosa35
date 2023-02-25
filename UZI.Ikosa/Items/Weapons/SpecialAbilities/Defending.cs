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
    [MagicAugmentRequirement(8, typeof(CraftMagicArmsAndArmorFeat), typeof(Shield), typeof(ShieldOfGrace))]
    public class Defending : WeaponSpecialAbility
    {
        public Defending()
            : base(typeof(Defending), 1, 0)
        {
        }

        public override object Clone()
        {
            return new Defending();
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"May trade attack for defense");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Defending;
    }
}
