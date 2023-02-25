using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(8, typeof(CraftMagicArmsAndArmorFeat))] // TODO: divine power
    public class MightyCleaving : WeaponSpecialAbility
    {
        public MightyCleaving(object source)
            : base(source,1,0)
        {
        }

        public override object Clone()
        {
            return new MightyCleaving(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                // TODO: description
                yield return new Description(Name, @"May make a cleave attack when dropping a target");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is MightyCleaving;
    }
}
