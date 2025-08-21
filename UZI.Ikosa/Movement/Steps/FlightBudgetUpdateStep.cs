using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FlightBudgetUpdateStep : MovementProcessStep
    {
        public FlightBudgetUpdateStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name { get { return @"Update flight budget "; } }

        protected override bool OnDoStep()
        {
            var _moveAct = MovementAction;
            if (_moveAct != null)
            {
                var _flight = _moveAct.Movement as FlightSuMovement;
                if (_flight != null)
                {
                    var _locator = Activity.Targets
                        .OfType<MovementLocatorTarget>()
                        .FirstOrDefault(_mlt => _mlt.Locator.ICore == Activity.Actor)
                        ?.Locator;
                    var _headingTarget = Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
                    var _idx = Activity.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
                    var _moveBudget = MovementAction.MovementBudget;
                    var _gravity = _locator?.GetGravityFace() ?? Visualize.AnchorFace.ZLow;
                    var _upAdjust = _headingTarget.GetUpDownAdjustment(_gravity, _idx.Value);
                    var _flightBudget = _flight.GetFlightBudget(Activity, _upAdjust);
                    var _moveHeading = _headingTarget.GetHeading(_gravity, _idx.Value);
                    var _turnHeading = (8 + _moveHeading - (_moveBudget.Heading ?? _moveHeading)) % 8;

                    #region half speed flag
                    // only figure this out the first time we take to flight per round
                    if (!_flightBudget.DistanceCovered.HasValue)
                    {
                        // start with no half-speed capacity
                        _flightBudget.HalfSpeed = false;
                        if (_moveAct.MovementRangeBudget.Double <= 0)
                        {
                            // no double capacity left
                            if (_moveAct.MovementRangeBudget.Remaining <= 1)
                            {
                                // and remaining is already in the last stretch
                                _flightBudget.HalfSpeed = true;
                            }
                        }
                        else
                        {
                            // double capacity left
                            if (!_moveAct.Budget.CanPerformBrief)
                            {
                                // but no capacity to perform another movement action
                                _flightBudget.HalfSpeed = true;
                            }
                        }
                    }
                    #endregion

                    #region forward distance
                    // calculate "forward" distance this move
                    double _dist = 0;
                    if ((_moveHeading % 2 == 0) || (_moveHeading < 0) || (_moveHeading > 7))
                    {
                        // non-diagonal
                        _dist = 5;
                    }
                    else
                    {
                        // diagonal
                        _dist = Math.Sqrt(2) * 5;
                    }
                    #endregion

                    // update distance += this move
                    _flightBudget.DistanceCovered = (_flightBudget.DistanceCovered ?? 0) + _dist;

                    // update since turn += this move
                    if (_turnHeading == 0)
                    {
                        _flightBudget.DistanceSinceTurn += _dist;
                    }
                    else
                    {
                        _flightBudget.DistanceSinceTurn = 0;
                    }

                    // update since down += this move
                    if (_upAdjust < UpDownAdjustment.Level)
                    {
                        _flightBudget.DistanceSinceDown = 0;
                    }
                    else
                    {
                        _flightBudget.DistanceSinceDown += _dist;
                    }

                    // update upwards crossings
                    if (_upAdjust > UpDownAdjustment.Upward)
                    {
                        _flightBudget.UpwardsCrossings += 1;
                    }
                    else
                    {
                        _flightBudget.UpwardsCrossings = 0;
                    }
                }
            }
            else
            {
                // no action provided
                Activity.Terminate(@"Incomplete Activity");
            }
            return true;
        }
    }
}
