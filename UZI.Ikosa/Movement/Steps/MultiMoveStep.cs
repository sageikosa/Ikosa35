using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class MultiMoveStep : MovementProcessStep
    {
        public MultiMoveStep(CoreActivity activity) :
            base(activity)
        {
        }

        public override string Name => @"Multi-step movement";

        protected override bool OnDoStep()
        {
            var _moveAct = MovementAction;
            if (_moveAct != null)
            {
                // determine if there should be additional activity for this destination target
                var _vector = StepDestinationTarget;
                var _idx = StepIndexTarget;
                if ((_vector != null) && (_idx != null))
                {

                    // are there more steps than we've used?
                    if (_vector.StepCount > (_idx.Value + 1))
                    {
                        var _activity = Activity;
                        // replace step index with new step index
                        AimTarget _replaceStepIndex(AimTarget target)
                            => target == _idx
                            ? new ValueTarget<int>(_idx.Key, _idx.Value + 1)
                            : target;

                        // build new activity
                        var _new = _moveAct.NextMoveInSequence(_activity,
                            _activity.OriginalTargets.Select(_t => _replaceStepIndex(_t)).ToList());
                        _moveAct.SetNext(_new, Process.ProcessManager as IkosaProcessManager);
                    }
                }
            }
            return true;
        }
    }
}
