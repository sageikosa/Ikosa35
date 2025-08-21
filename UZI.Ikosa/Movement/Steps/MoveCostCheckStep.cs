using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class MoveCostCheckStep : MovementProcessStep
    {
        public MoveCostCheckStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Calculate movement cost";

        protected override bool OnDoStep()
        {
            var _activity = Activity;
            if (_activity != null)
            {
                var _cost = MovementAction.BaseCost;
                var _vector = StepDestinationTarget;
                var _idx = StepIndexTarget;
                var _moveAct = MovementAction;
                if ((_vector != null) && (_moveAct != null) && (_idx != null))
                {
                    var _finalCost = _moveAct.Movement.MoveCost(_activity, _cost);
                    var _locator = Activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .FirstOrDefault(_mlt => _mlt?.Value.Locator.ICore == Activity.Actor)
                        ?.Value.Locator;
                    _locator ??= Activity.Actor.GetLocated()?.Locator;

                    // see if range is available
                    // diagonal if heading on an odd value, or not solely moving in the up/down direction
                    var _gravity = _locator?.GetGravityFace() ?? Visualize.AnchorFace.ZLow;
                    var _diagonal = ((_vector.GetHeading(_gravity, _idx.Value) % 2) == 1)
                        || _vector.GetUpDownAdjustment(_gravity, _idx.Value).IsDiagonal();
                    if (_moveAct.MovementRangeBudget.CanMove(_moveAct.Movement, _diagonal, _finalCost))
                    {
                        // good to go!
                        _activity.SetFirstTarget(new ValueTarget<bool>(MovementTargets.Move_Diagonal, _diagonal));
                        _activity.SetFirstTarget(new ValueTarget<double>(MovementTargets.Move_Cost, _finalCost));
                        foreach (var _step in MovementAction.DoMoveCostCheck(this, _finalCost))
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
            _activity.Terminate(@"Incomplete or missing activity");
            return true;
        }
    }
}
