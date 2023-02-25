using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Actions;
using System.Collections.Generic;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Base class for Influences that give boosts to spells with descriptors as an Influence benefit.</summary>
    [Serializable]
    public abstract class DescriptorInfluence<Descript> : Influence, IQualifyDelta
        where Descript : Descriptor
    {
        #region construction
        protected DescriptorInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass, string boostDescription)
            : base(devotion, influenceClass)
        {
            _Boost = new QualifyingDelta(1, GetType(), boostDescription);
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private IDelta _Boost;
        private TerminateController _Term;
        #endregion

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
            if (qualify?.Actor == Creature)
            {
                if ((qualify?.Source as SpellSource)
                    ?.SpellDef
                    ?.Descriptors.Any(_d => _d is Descript) ?? false)
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
    }
}
