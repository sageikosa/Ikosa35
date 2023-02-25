using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class QuietArmor : SkillfulArmorAdjunct
    {
        protected QuietArmor(object source, int amount)
            : base(source, amount)
        {
        }

        protected override Type SkillType()
        {
            return typeof(SilentStealthSkill);
        }
    }

    [Serializable]
    public class QuietArmorLow : QuietArmor
    {
        public QuietArmorLow(object source)
            : base(source, 5)
        {
        }
        public override object Clone()
        {
            return new QuietArmorLow(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Quiet Lesser", @"+5 Silent Stealth");
                yield break;
            }
        }
    }

    [Serializable]
    public class QuietArmorMedium : QuietArmor
    {
        public QuietArmorMedium(object source)
            : base(source, 10)
        {
        }
        public override object Clone()
        {
            return new QuietArmorMedium(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Quiet", @"+10 Silent Stealth");
                yield break;
            }
        }
    }

    [Serializable]
    public class QuietArmorHigh : QuietArmor
    {
        public QuietArmorHigh(object source)
            : base(source, 15)
        {
        }
        public override object Clone()
        {
            return new QuietArmorHigh(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Quiet Greater", @"+15 Silent Stealth");
                yield break;
            }
        }
    }
}
