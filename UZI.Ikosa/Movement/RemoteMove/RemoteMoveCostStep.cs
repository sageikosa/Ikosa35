using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveCostStep : RemoteMoveStep
    {
        public RemoteMoveCostStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Account for movement cost";

        protected override bool OnDoStep()
        {
            var _remoteMove = RemoteMoveAction;
            if (_remoteMove != null)
            {
                var _activity = Activity;
                var _locator = _activity.Targets
                    .OfType<MovementLocatorTarget>()
                    .FirstOrDefault(_mlt => _mlt.Locator.ICore == _activity.Actor)
                    ?.Locator ?? _activity.GetFirstTarget<ValueTarget<CoreObject>>(@"Mover")?.Value.GetLocated()?.Locator;

                var _vector = StepDestinationTarget;
                var _idx = StepIndexTarget;
                var _diagonal = _activity.GetFirstTarget<ValueTarget<bool>>(MovementTargets.Move_Diagonal);
                var _cost = _activity.GetFirstTarget<ValueTarget<double>>(MovementTargets.Move_Cost);
                if ((_vector != null) && (_diagonal != null) && (_cost != null) && (_idx != null))
                {
                    _remoteMove.RemoteMoveGroup.Target.DoMove(_diagonal.Value, _cost.Value);
                }
                else
                {
                    _activity.Terminate(@"Missing Targets");
                }
            }
            else
            {
                Activity?.Terminate(@"Incomplete Activity");
            }
            return true;
        }
    }
}
