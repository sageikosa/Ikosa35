using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DeltaTrait : TraitEffect
    {
        public DeltaTrait(ITraitSource traitSource, IModifier modifier, params Deltable[] targets)
            : base(traitSource)
        {
            _Targets = targets.ToList();
            _Delta = modifier;
        }

        #region data
        private List<Deltable> _Targets;
        private IModifier _Delta;
        #endregion

        public IModifier Modifier => _Delta;
        public IEnumerable<Deltable> Targets => _Targets.Select(_t => _t);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            foreach (var _target in _Targets)
                _target.Deltas.Add(_Delta);
        }

        protected override void OnDeactivate(object source)
        {
            _Delta.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new DeltaTrait(TraitSource, _Delta, _Targets.ToArray());
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            return new DeltaTrait(traitSource, _Delta, _Targets.ToArray());
        }
    }
}