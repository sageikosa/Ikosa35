using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Limited single step movement that can not be combined with other movements in a turn.</summary>
    [Serializable]
    public class ShiftPosition : MovementAction
    {
        #region construction
        /// <summary>Limited single step movement that can not be combined with other movements in a turn.</summary>
        public ShiftPosition(MovementBase movement, bool provokesMelee)
            : base(movement, new ActionTime(TimeType.FreeOnTurn), provokesMelee)
        {
        }
        #endregion

        public override string Key => @"Movement.ShiftPosition";
        public override string DisplayName(CoreActor actor) => $@"Shift position ({Movement.Name})";

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // first check base (effort and movement left)
            var _response = base.CanPerformNow(budget);
            if (!_response.Success)
            {
                return _response;
            }

            // grab movement budget for updating and checking
            if (this.MovementBudget.HasMoved)
            {
                // cannot shift position if moved already...
                return new ActivityResponse(false);
            }

            if (Movement.EffectiveValue <= 5)
            {
                // speed less than or equal to 5 cannot shift
                return new ActivityResponse(false);
            }

            return new ActivityResponse(true);
        }
        #endregion

        public override bool IsStackBase(CoreActivity activity) => false;

        #region public override IEnumerable<CoreStep> OnMoveCostCheck(MoveCostCheckStep step, double finalCost)
        public override IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
        {
            // register that movement has been made
            yield return new HasMovedStep(step.Activity);
            if (finalCost <= 1)
            {
                // block further movement
                yield return new CanStillMoveStep(step.Activity, MovementBudget);

                // ensure not immobilized
                if (!(step.Activity.GetFirstTarget<ValueTarget<bool>>(@"Immobilized")?.Value ?? false))
                {
                    // make the actual relocation
                    foreach (var _moveLoc in step.Activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .Select(_vt => _vt.Value))
                    {
                        yield return new RelocationStep(step.Activity, _moveLoc.Locator, _moveLoc.TargetRegion,
                            Movement, _moveLoc.Offset,
                            AnchorFaceListHelper.Create(step.StepDestinationTarget.CrossingFaces(
                                _moveLoc.Locator.GetGravityFace(),
                                step.StepIndexTarget?.Value ?? 0)), _moveLoc.BaseFace);
                    }
                }
            }
            else
            {
                yield return step.Activity.GetActivityResultNotifyStep(@"Too much movement for a shift");
            }
            yield break;
        }
        #endregion

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets) => null;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
