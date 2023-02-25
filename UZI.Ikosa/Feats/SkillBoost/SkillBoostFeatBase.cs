using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public abstract class SkillBoostFeatBase: FeatBase 
    {
        protected SkillBoostFeatBase(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override void OnAdd()
        {
            base.OnAdd();

            _Modifier = new Delta(2, this.GetType());

            // add skill boosts
            foreach (Type _skillType in this.SkillTypes())
            {
                // modify the skill
                _Creature.Skills[_skillType].Deltas.Add(_Modifier);
            }
        }

        protected Delta _Modifier;

        protected abstract IEnumerable<Type> SkillTypes();

        protected override void OnRemove()
        {
            base.OnRemove();

            // remove skill boosts
            _Modifier.DoTerminate();
        }
    }
}
