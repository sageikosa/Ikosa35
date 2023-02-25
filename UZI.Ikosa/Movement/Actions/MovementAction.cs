using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;
using System.Diagnostics;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class MovementAction : ActionBase
    {
        protected MovementAction(MovementBase movement, ActionTime actionCost, bool provokesMelee)
            : base(movement, actionCost, provokesMelee, false, @"200")
        {
        }

        #region state
        protected MovementBudget _MoveBudget = null;
        protected MovementRangeBudget _MoveRange = null;
        protected CoreActivity _Next = null;
        protected IkosaProcessManager _ProcMgr = null;
        protected bool _IsValid = false;
        #endregion

        public MovementBase Movement => Source as MovementBase;
        public MovementBudget MovementBudget => _MoveBudget;
        public MovementRangeBudget MovementRangeBudget => _MoveRange;
        public override string DisplayName(CoreActor actor) => @"Movement";
        public virtual double BaseCost => 1;

        public Creature Creature
            => Movement?.CoreObject as Creature;

        /// <summary>Generally, a move is solitary if it takes total effort.  Exception: adjustment</summary>
        public virtual bool SolitaryMove
            => TimeCost.ActionTimeType == TimeType.Total;

        #region protected IEnumerable<CoreStep> BaseOnMoveCostCheck(MoveCostCheckStep step, double finalCost)
        /// <summary>RegisterActivityStep, HasMovedStep, MoveCostStep, and RelocationStep</summary>
        protected IEnumerable<CoreStep> BaseOnMoveCostCheck(MoveCostCheckStep step, double finalCost)
        {
            if (step != null)
            {
                yield return new RegisterActivityStep(step.Activity, Budget);
                yield return new HasMovedStep(step.Activity);
                yield return new MoveCostStep(step.Activity);

                // ensure not immobilized
                if (!(step.Activity.GetFirstTarget<ValueTarget<bool>>(MovementTargets.Immobilized)?.Value ?? false))
                {
                    // before relocate, check the interaction
                    foreach (var _moveLoc in step.Activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .Select(_vt => _vt.Value))
                    {
                        if (_moveLoc.Locator.ICore is IInteract _interactor)
                        {
                            var _preLocate = new Interaction(step.Activity.Actor, this, _interactor,
                                new PreRelocateData(step.Activity.Actor, _moveLoc.Locator, _moveLoc.TargetRegion,
                                    Movement, _moveLoc.Offset,
                                    AnchorFaceListHelper.Create(step.StepDestinationTarget.CrossingFaces(
                                        _moveLoc.Locator.GetGravityFace(),
                                        step.StepIndexTarget?.Value ?? 0)),
                                    _moveLoc.BaseFace));
                            _interactor.HandleInteraction(_preLocate);
                            var _react = _preLocate.Feedback.OfType<PreRelocateFeedback>().SelectMany(_prf => _prf.ReactiveSteps)
                                .ToList();
                            if (_react.Any())
                            {
                                foreach (var _step in _react)
                                {
                                    yield return _step;
                                }
                            }
                        }
                    }

                    foreach (var _moveLoc in step.Activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .Select(_vt => _vt.Value))
                    {
                        yield return new RelocationStep(step.Activity, _moveLoc.Locator, _moveLoc.TargetRegion,
                            Movement, _moveLoc.Offset,
                            AnchorFaceListHelper.Create(step.StepDestinationTarget.CrossingFaces(
                                _moveLoc.Locator.GetGravityFace(),
                                step.StepIndexTarget?.Value ?? 0)),
                            _moveLoc.BaseFace);
                    }
                }
                yield return new MultiMoveStep(step.Activity);
            }
            yield break;
        }
        #endregion

        /// <summary>returns a follow-on activity if the StepDestinationTarget indicates multiple steps</summary>
        public abstract CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets);

        #region protected IEnumerable<CoreStep> DiagonalOnMoveCostCheck(MoveCostCheckStep step, double finalCost, bool atStart)
        /// <summary>
        /// <para>calculates additional time requirements for standard movement</para>
        /// <para>usually not used by move actions that already handle time differently</para>
        /// </summary>
        protected IEnumerable<CoreStep> DiagonalOnMoveCostCheck(MoveCostCheckStep step, double finalCost, bool atStart)
        {
            // alters time if needs a double move to start
            var _diagonal = step.Activity.GetFirstTarget<ValueTarget<bool>>(MovementTargets.Move_Diagonal);
            if (_diagonal != null)
            {
                // see if double move is required
                if (MovementRangeBudget.RequiresDouble(Movement, _diagonal.Value, finalCost))
                {
                    if (atStart)
                    {
                        // budget for double move when starting movement for the round?
                        if (Budget.CanPerformTotal)
                        {
                            TimeCost = new ActionTime(TimeType.Total);
                        }
                        else
                        {
                            yield return step.Activity.GetActivityResultNotifyStep(@"Too much effort this time around");
                            yield break;
                        }
                    }

                    // budget left (and chosen option) for double move when continuing movement?
                    var _dblMove = step.Activity.GetFirstTarget<OptionTarget>(MovementTargets.Double);
                    if ((_dblMove != null)
                        && _dblMove.Option.Key.Equals(@"True", StringComparison.OrdinalIgnoreCase)
                        && Budget.CanPerformBrief)
                    {
                        TimeCost = new ActionTime(TimeType.Brief);
                    }
                    else
                    {
                        yield return step.Activity.GetActivityResultNotifyStep(@"Too much effort this time around");
                        yield break;
                    }
                }

                // normal post move check stuff
                foreach (var _step in BaseOnMoveCostCheck(step, finalCost))
                    yield return _step;
            }
            else
            {
                yield return step.Activity.GetActivityResultNotifyStep(@"Incomplete activity");
            }
            yield break;
        }
        #endregion

        public virtual IEnumerable<CoreStep> DoMoveCostCheck(MoveCostCheckStep step, double finalCost)
            => BaseOnMoveCostCheck(step, finalCost);

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // first check effort budget
            var _response = base.CanPerformNow(budget);
            if (!_response.Success)
                return _response;

            // grab movement budget for updating
            _MoveBudget = MovementBudget.GetBudget(budget);
            _MoveRange = MovementRangeBudget.GetBudget(budget);

            // see if the movement budget still allows movement
            if (MovementBudget.CanStillMove)
            {
                // needs solitary movement: must not have moved
                if (SolitaryMove)
                    return new ActivityResponse(!MovementBudget.HasMoved);
                return new ActivityResponse(true);
            }

            // no movement left...
            return new ActivityResponse(false);
        }
        #endregion

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(Movement.Name, activity.Actor, observer);

        #region protected static IEnumerable<OptionAimOption> DoubleOptions()
        protected static IEnumerable<OptionAimOption> DoubleOptions()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Allows double move if needed (and possible)",
                Name = @"Allow Double",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Prevents double move if needed",
                Name = @"Deny Double",
                Value = false
            };
            yield break;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new VolumeAim(MovementTargets.LeadCell, @"Lead Cell", new FixedRange(0), FixedRange.One, new FixedRange(0), new FixedRange(1));
            yield return new MovementAim(MovementTargets.Direction, @"Direction");
            foreach (var _aim in Movement.GetAimingModes(activity))
                yield return _aim;
            yield break;
        }
        #endregion

        #region public override void ProcessManagerInitialized(CoreActivity activity)
        public override void ProcessManagerInitialized(CoreActivity activity)
        {
            // setup extra target information early
            _IsValid = Movement.IsValidActivity(activity, activity.MainObject);
            base.ProcessManagerInitialized(activity);
        }
        #endregion

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // then make sure sufficient ranged budget is available...
            return Movement.CostFactorStep(activity);
        }
        #endregion

        #region public void SetNext(CoreActivity activity, IkosaProcessManager manager)
        /// <summary>Set next activity when process is stopping</summary>
        public void SetNext(CoreActivity activity, IkosaProcessManager manager)
        {
            _Next = activity;
            _ProcMgr = manager;
        }
        #endregion

        #region public override void ActivityFinalization(CoreActivity activity, bool deactivated)
        public override void ActivityFinalization(CoreActivity activity, bool deactivated)
        {
            if (!deactivated)
            {
                // when process is stopping do next action if defined
                var _next = _Next;
                _Next = null;
                if (_next != null)
                {
                    Budget.DoAction(_ProcMgr, _next);
                }
                Creature.SendSysNotify(new RefreshNotify(true, false, false, false, false));
            }
            base.ActivityFinalization(activity, deactivated);
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
        {
            // true if moving regions overlap
            // TODO: friendly or dis-inclined...
            var _rgn = potentialTarget?.GetLocated()?.Locator?.GeometricRegion;
            if (_rgn != null)
            {
                return activity.Targets.OfType<MovementLocatorTarget>()
                    .Any(_mlt => _mlt.TargetRegion?.ContainsGeometricRegion(_rgn) ?? false);
            }
            return false;
        }

        public override void DoClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            // finished movement, see if must move to legal position
            var _loc = Movement?.CoreObject?.GetLocated()?.Locator;
            if (!(Movement?.IsLegalPosition(_loc) ?? false))
            {
                if (_loc != null)
                {
                    var _last = _loc.LastLegalRegion;
                    if (_last == null)
                    {
                        // no last legal region, squeeze or fall prone
                        // TODO: friendly squeeze or fall prone
                    }
                    else
                    {
                        // relocate to last legal position
                        _loc.MovementCrossings = _loc.MovementCrossings.Invert();
                        _loc.IntraModelOffset = _loc.LastModelOffset;
                        Movement?.OnPreRelocated(activity, _loc);
                        _loc.Relocate(_last, _loc.PlanarPresence);
                        Movement?.OnRelocated(activity, _loc);
                    }
                }
            }
            base.DoClearStack(budget, activity);
        }
    }

    public static class MovementTargets
    {
        public const string Double = @"Double";
        public const string Direction = @"Direction";
        public const string Immobilized = @"Immobilized";
        public const string LeadCell = @"LeadCell";
        public const string MoveData = @"MoveData";
        public const string StepIndex = @"StepIndex";
        public const string Move_Cost = @"Move.Cost";
        public const string Move_Diagonal = @"Move.Diagonal";
        public const string IMoveAlterer_BlocksTransit = @"IMoveAlterer.BlocksTransit";
    }
}
