using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Run", true)
    ]
    public class RunFeat : FeatBase
    {
        public RunFeat(object source, int powerLevel) 
            : base(source, powerLevel)
        {
        }

        #region state
        protected Delta _Delta;
        protected AdjunctBlocker<RunPenalty> _Blocker;
        #endregion

        public override string Benefit 
            => @"Increased run speed multiplier.  +4 Jump after running start.  DEX bonus to armor rating while running.";

        protected override void OnAdd()
        {
            base.OnAdd();

            // run cost
            _Delta = new Delta(1, typeof(RunFeat), @"+1 Run Range");
            _Creature.RunFactor.Deltas.Add(_Delta);

            // TODO: jump boost in same round as running

            // suppress run-penalty adjunct
            _Blocker = new AdjunctBlocker<RunPenalty>(this);
            _Creature.AddAdjunct(_Blocker);
        }

        protected override void OnRemove()
        {
            base.OnRemove();

            // run cost
            _Delta.DoTerminate();

            // TODO: remove jump boost in same round as running

            // remove suppression of run-penalty adjunct
            _Blocker.Eject();
        }
    }
}
