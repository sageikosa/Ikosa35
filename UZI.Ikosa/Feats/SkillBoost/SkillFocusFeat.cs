using System;
using Uzi.Ikosa.Skills;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    ParameterizedFeatInfo("Skill Focus", "+3 bonus", typeof(SkillLister))
    ]
    public class SkillFocusFeat<Skl> : FeatBase where Skl : SkillBase
    {
        public SkillFocusFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Name { get { return string.Format("SkillFocus: {0}", typeof(Skl).Name); } }
        public override string Benefit { get { return string.Format("+3 bonus on {0}", typeof(Skl).Name); } }

        protected Delta _Modifier;
        protected override void OnAdd()
        {
            base.OnAdd();

            // modify the skill
            _Modifier = new Delta(3, this.GetType());
            _Creature.Skills[typeof(Skl)].Deltas.Add(_Modifier);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _Modifier.DoTerminate();
        }
    }
}
