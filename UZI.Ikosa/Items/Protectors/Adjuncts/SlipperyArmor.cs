using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class SlipperyArmor : SkillfulArmorAdjunct
    {
        protected SlipperyArmor(int amount)
            : base(typeof(SlipperyArmor), amount)
        {
        }

        protected override Type SkillType()
        {
            return typeof(EscapeArtistSkill);
        }
    }

    [Serializable]
    [MagicAugmentRequirement(5, typeof(CraftMagicArmsAndArmorFeat), typeof(SlipperySurface))]
    public class SlipperyArmorLow : SlipperyArmor
    {
        public SlipperyArmorLow()
            : base(5)
        {
        }
        public override object Clone()
        {
            return new SlipperyArmorLow();
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Slippery Lesser", @"+5 Escape");
                yield break;
            }
        }
    }

    [Serializable]
    [MagicAugmentRequirement(10, typeof(CraftMagicArmsAndArmorFeat), typeof(SlipperySurface))]
    public class SlipperyArmorMedium : SlipperyArmor
    {
        public SlipperyArmorMedium()
            : base(10)
        {
        }
        public override object Clone()
        {
            return new SlipperyArmorMedium();
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Slippery", @"+10 Escape");
                yield break;
            }
        }
    }

    [Serializable]
    [MagicAugmentRequirement(15, typeof(CraftMagicArmsAndArmorFeat), typeof(SlipperySurface))]
    public class SlipperyArmorHigh : SlipperyArmor
    {
        public SlipperyArmorHigh()
            : base(15)
        {
        }
        public override object Clone()
        {
            return new SlipperyArmorHigh();
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Slippery Lesser", @"+15 Escape");
                yield break;
            }
        }
    }
}
