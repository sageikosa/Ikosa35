using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class DivinationInfluence : Influence, IQualifyDelta, IClassSkills
    {
        #region construction
        protected DivinationInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _Boost = new QualifyingDelta(1, typeof(DivinationInfluence), @"Divination Influence");
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private IDelta _Boost;
        private TerminateController _Term;
        #endregion

        public override string Name => @"Divination Influence";
        public override string Description => @"+1 Caster Level for Divination Spells.  All Knowledge skills are class skills.";
        public override object Clone() => new DivinationInfluence(Devotion, InfluenceClass);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // boost caster level for descriptor spells (only affects influence class)
            Creature.ExtraClassPowerLevel.Deltas.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            // terminate the IQualifyDelta
            DoTerminate();
            base.OnDeactivate(source);
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify.Actor == Creature)
            {
                if ((qualify.Source is SpellSource _spellSource)
                    && (_spellSource.MagicStyle is Divination))
                {
                    yield return _Boost;
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate() { _Term.DoTerminate(); }
        public void AddTerminateDependent(IDependOnTerminate subscriber) { _Term.AddTerminateDependent(subscriber); }
        public void RemoveTerminateDependent(IDependOnTerminate subscriber) { _Term.RemoveTerminateDependent(subscriber); }
        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        #region IClassSkills Members

        public bool IsClassSkill(AdvancementClass advClass, SkillBase skill)
        {
            // must be a knowledge skill, class must be primary influence class, and its influences must contain this
            return typeof(KnowledgeSkill<>).IsAssignableFrom(skill.GetType())
                && (advClass is IPrimaryInfluenceClass)
                && (advClass as IPrimaryInfluenceClass).Influences.Contains(this);
        }

        #endregion
    }
}
