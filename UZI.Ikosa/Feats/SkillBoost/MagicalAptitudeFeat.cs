using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Magical Aptitude")
    ]
    public class MagicalAptitudeFeat : SkillBoostFeatBase
    {
        public MagicalAptitudeFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(SpellcraftSkill);
            yield return typeof(UseMagicItemSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Spellcraft and +2 Use Magic Device"; }
        }
    }
}
