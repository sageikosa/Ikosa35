using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RunPenalty : Adjunct
    {
        public RunPenalty()
            : base(typeof(RunPenalty))
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Anchor.AddAdjunct(new UnpreparedToDodge(this));
        }

        protected override void OnDeactivate(object source)
        {
            Anchor.Adjuncts.OfType<UnpreparedToDodge>()
                .FirstOrDefault(_utd => _utd.Source == this)?.Eject();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new RunPenalty();
    }
}
