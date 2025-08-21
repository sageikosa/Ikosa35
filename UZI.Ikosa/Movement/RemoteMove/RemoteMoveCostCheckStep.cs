using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveCostCheckStep : RemoteMoveStep
    {
        public RemoteMoveCostCheckStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Ensure budget for move";

        protected override bool OnDoStep()
        {
            var _remoteMove = RemoteMoveAction;
            if (_remoteMove != null)
            {
                var _vector = StepDestinationTarget;
                var _idx = StepIndexTarget;
                if ((_vector != null) && (_idx != null))
                {
                    var _activity = Activity;
                    var _finalCost = 1d;
                    var _locator = _activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .FirstOrDefault(_mlt => _mlt?.Value.Locator.ICore == Activity.Actor)
                        ?.Value.Locator;
                    _locator ??= _activity.Actor.GetLocated()?.Locator;

                    // see if range is available
                    // diagonal if heading on an odd value, or not solely moving in the up/down direction
                    var _gravity = _locator?.GetGravityFace() ?? Visualize.AnchorFace.ZLow;
                    var _diagonal = ((_vector.GetHeading(_gravity, _idx.Value) % 2) == 1)
                        || _vector.GetUpDownAdjustment(_gravity, _idx.Value).IsDiagonal();
                    if (_remoteMove.RemoteMoveGroup.Target.CanMove(_diagonal, _finalCost))
                    {
                        // good to go!
                        _activity.SetFirstTarget(new ValueTarget<bool>(MovementTargets.Move_Diagonal, _diagonal));
                        _activity.SetFirstTarget(new ValueTarget<double>(MovementTargets.Move_Cost, _finalCost));
                        foreach (var _step in RemoteMoveAction.DoMoveCostCheck(this, _finalCost))
                        {
                            AppendFollowing(_step);
                        }

                        return true;
                    }
                    else
                    {
                        // insufficient range (react by terminating the process)
                        _activity.Terminate(@"Not enough movement left");
                        return true;
                    }
                }
            }

            // failing! (react by terminating the process)
            Activity?.Terminate(@"Incomplete or missing activity");
            return true;
        }
    }
}
