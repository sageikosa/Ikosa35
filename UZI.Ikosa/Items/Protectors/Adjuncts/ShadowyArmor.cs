using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class ShadowyArmor : SkillfulArmorAdjunct
    {
        protected ShadowyArmor(object source, int amount)
            : base(source, amount)
        {
        }

        protected override Type SkillType()
        {
            return typeof(StealthSkill);
        }
    }

    [Serializable]
    public class ShadowyArmorLow : ShadowyArmor
    {
        public ShadowyArmorLow(object source)
            : base(source, 5)
        {
        }
        public override object Clone()
        {
            return new ShadowyArmorLow(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Shadowy Lesser", @"+5 Stealth");
                yield break;
            }
        }
    }

    [Serializable]
    public class ShadowyArmorMedium : ShadowyArmor
    {
        public ShadowyArmorMedium(object source)
            : base(source, 10)
        {
        }
        public override object Clone()
        {
            return new ShadowyArmorMedium(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Shadowy", @"+10 Stealth");
                yield break;
            }
        }
    }

    [Serializable]
    public class ShadowyArmorHigh : ShadowyArmor
    {
        public ShadowyArmorHigh(object source)
            : base(source, 15)
        {
        }
        public override object Clone()
        {
            return new ShadowyArmorHigh(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(@"Shadowy Greater", @"+15 Stealth");
                yield break;
            }
        }
    }
}
