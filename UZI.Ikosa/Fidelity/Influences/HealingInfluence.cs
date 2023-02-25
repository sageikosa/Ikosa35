using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class HealingInfluence : Influence, IQualifyDelta
    {
        #region construction
        public HealingInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _HealBoost = new QualifyingDelta(1, typeof(HealingInfluence), @"Healing Influence");
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private IDelta _HealBoost;
        private TerminateController _Term;
        #endregion

        public override string Name => @"Healing Influence";
        public override string Description => @"+1 Caster Level for Healing Spells";
        public override object Clone() => new HealingInfluence(Devotion, InfluenceClass);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // boost caster level for healing spells
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
                    && (_spellSource.MagicStyle is Conjuration)
                    && ((_spellSource.MagicStyle as Conjuration).SubStyle == Conjuration.SubConjure.Healing))
                {
                    yield return _HealBoost;
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
    }
}