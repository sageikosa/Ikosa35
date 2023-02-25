using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMultiMoveStep : RemoteMoveStep
    {
        public RemoteMultiMoveStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Multi-step movement";

        protected override bool OnDoStep()
        {
            var _remoteMove = RemoteMoveAction;
            if (_remoteMove != null)
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
                        var _new = new CoreActivity(_activity.Actor, new RemoteMoveAction(_remoteMove.RemoteMoveGroup),
                            _activity.OriginalTargets.Select(_t => _replaceStepIndex(_t)).ToList());
                        _remoteMove.SetNext(_new, Process.ProcessManager as IkosaProcessManager);
                    }
                }
            }
            return true;
        }
    }
}
