using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using System.Diagnostics;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class MoveCostStep : MovementProcessStep
    {
        public MoveCostStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Account for movement cost";

        protected override bool OnDoStep()
        {
            var _moveAct = MovementAction;
            if (_moveAct != null)
            {
                var _locator = Activity.Targets
                    .OfType<MovementLocatorTarget>()
                    .FirstOrDefault(_mlt => _mlt.Locator.ICore == Activity.Actor)
                    ?.Locator ?? Activity.GetFirstTarget<ValueTarget<CoreObject>>(@"Mover")?.Value.GetLocated()?.Locator;
                var _vector = StepDestinationTarget;
                var _idx = StepIndexTarget;
                var _diagonal = Activity.GetFirstTarget<ValueTarget<bool>>(MovementTargets.Move_Diagonal);
                var _cost = Activity.GetFirstTarget<ValueTarget<double>>(MovementTargets.Move_Cost);
                if ((_vector != null) && (_diagonal != null) && (_cost != null) && (_idx != null))
                {
                    _moveAct.MovementBudget.Heading = _vector.GetHeading(_locator?.GetGravityFace() ?? Visualize.AnchorFace.ZLow, _idx.Value);
                    _moveAct.MovementRangeBudget.DoMove(_moveAct, _diagonal.Value, _cost.Value);
                    var _blocker = Activity.GetFirstTarget<ValueTarget<IMoveAlterer>>(MovementTargets.IMoveAlterer_BlocksTransit);
                    if (_blocker != null)
                    {
                        BumpAwareness.CreateBumpAwareness(
                            Activity.Actor as Creature,
                            _blocker.Value as IAdjunctable,
                            new[] { _blocker.Value.ID });
                    }
                }
                else
                {
                    Activity.Terminate(@"Missing Targets");
                }
            }
            else
            {
                Activity.Terminate(@"Incomplete Activity");
            }
            return true;
        }
    }
}
