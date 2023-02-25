using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class QualifyDeltaTrait : TraitEffect
    {
        public QualifyDeltaTrait(ITraitSource traitSource, 
            IQualifyDelta qualifier, params Deltable[] targets)
            : base(traitSource)
        {
            _Qualifier = qualifier;
            _Targets = targets.ToList();
        }

        #region private data
        private List<Deltable> _Targets;
        private IQualifyDelta _Qualifier;
        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            foreach (var _target in _Targets)
            {
                _target.Deltas.Add(_Qualifier);
            }
        }

        protected override void OnDeactivate(object source)
        {
            _Qualifier.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new QualifyDeltaTrait(TraitSource, _Qualifier, _Targets.ToArray());
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new QualifyDeltaTrait(traitSource, _Qualifier, _Targets.ToArray());
        }
    }
}
