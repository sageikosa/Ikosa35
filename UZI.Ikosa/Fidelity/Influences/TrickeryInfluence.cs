using System;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class TrickeryInfluence : Influence, IClassSkills
    {
        public TrickeryInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
        }

        public override string Name { get { return @"Trickery Influence"; } }
        public override string Description { get { return @"Bluff, Disguise and Stealth are class skills"; } }
        public override object Clone() { return new TrickeryInfluence(Devotion, InfluenceClass); }

        #region IClassSkills Members

        public bool IsClassSkill(AdvancementClass advClass, SkillBase skill)
        {
            return ((skill is BluffSkill) || (skill is DisguiseSkill) || (skill is StealthSkill))
                && (advClass is IPrimaryInfluenceClass)
                && (advClass as IPrimaryInfluenceClass).Influences.Contains(this);
        }

        #endregion
    }
}
