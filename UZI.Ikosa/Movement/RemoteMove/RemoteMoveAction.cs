using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveAction : ActionBase
    {
        public RemoteMoveAction(RemoteMoveGroup remoteMoveGroup)
            : base(remoteMoveGroup, new ActionTime(Contracts.TimeType.SubAction), false, false, @"202")
        {
        }

        protected RemoteMoveAction(RemoteMoveGroup remoteMoveGroup, ActionTime actionTime, string orderKey)
            : base(remoteMoveGroup, actionTime, false, false, orderKey)
        {
        }

        #region state
        protected CoreActivity _Next = null;
        protected IkosaProcessManager _ProcMgr = null;
        protected bool _IsValid = false;
        #endregion

        /// <summary>RemoteMoveAction sourced by RemoteMoveGroup</summary>
        public RemoteMoveGroup RemoteMoveGroup => Source as RemoteMoveGroup;

        /// <summary>Remove move actions are effectively mental</summary>
        public override bool IsMental => true;

        public override string Key => @"RemoteMove.Continue";

        // TODO:
        public override string DisplayName(CoreActor observer) => @"Remotely Move {target}";

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // first check effort budget
            var _response = base.CanPerformNow(budget);
            if (!_response.Success)
                return _response;

            if (RemoteMoveGroup.Target.AnyMoveLeft)
            {
                return new ActivityResponse(true);
            }

            // no movement left...
            return new ActivityResponse(false);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new MovementAim(MovementTargets.Direction, @"Direction");
            yield break;
        }

        // TODO:
        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Remotely Move", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => TimeCost.ActionTimeType > TimeType.SubAction;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => TimeCost.ActionTimeType > TimeType.SubAction;

        public override void ProcessManagerInitialized(CoreActivity activity)
        {
            // unlike normal move actions, remote move does not move the actor...
            _IsValid = RemoteMoveGroup.Movement.IsValidActivity(activity, RemoteMoveGroup.Movement.CoreObject);
            base.ProcessManagerInitialized(activity);
        }

        public StepDestinationTarget StepDestinationTarget(CoreActivity activity)
            => activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);

        public ValueTarget<int> StepIndexTarget(CoreActivity activity)
            => activity.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);

        #region public void SetNext(CoreActivity activity, IkosaProcessManager manager)
        /// <summary>Set next activity when process is stopping</summary>
        public void SetNext(CoreActivity activity, IkosaProcessManager manager)
        {
            _Next = activity;
            _ProcMgr = manager;
        }
        #endregion

        #region public IEnumerable<CoreStep> DoMoveCostCheck(RemoteMoveCostCheckStep step, double finalCost)
        /// <summary>RegisterActivityStep, RemoteMoveCostStep, and RelocationStep</summary>
        public IEnumerable<CoreStep> DoMoveCostCheck(RemoteMoveCostCheckStep step, double finalCost)
        {
            if (step != null)
            {
                var _activity = step.Activity;
                yield return new RegisterActivityStep(_activity, Budget);
                yield return new RemoteMoveCostStep(_activity);

                var _movement = RemoteMoveGroup.Movement;

                // ensure not immobilized
                if (!(_activity.GetFirstTarget<ValueTarget<bool>>(MovementTargets.Immobilized)?.Value ?? false))
                {
                    // before relocate, check the interaction
                    foreach (var _moveLoc in _activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .Select(_vt => _vt.Value))
                    {
                        if (_moveLoc.Locator.ICore is IInteract _interactor)
                        {
                            var _preLocate = new Interaction(_activity.Actor, this, _interactor,
                                new PreRelocateData(_activity.Actor, _moveLoc.Locator, _moveLoc.TargetRegion,
                                    _movement, _moveLoc.Offset,
                                    AnchorFaceListHelper.Create(StepDestinationTarget(_activity).CrossingFaces(
                                        _moveLoc.Locator.GetGravityFace(),
                                        StepIndexTarget(_activity)?.Value ?? 0)),
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

                    foreach (var _moveLoc in _activity.Targets
                        .OfType<ValueTarget<MovementLocatorTarget>>()
                        .Select(_vt => _vt.Value))
                    {
                        yield return new RelocationStep(_activity, _moveLoc.Locator, _moveLoc.TargetRegion,
                            _movement, _moveLoc.Offset,
                            AnchorFaceListHelper.Create(StepDestinationTarget(_activity).CrossingFaces(
                                _moveLoc.Locator.GetGravityFace(),
                                StepIndexTarget(_activity)?.Value ?? 0)),
                            _moveLoc.BaseFace);
                    }
                }

                // remote multi move next step
                yield return new RemoteMultiMoveStep(step.Activity);
            }
            yield break;
        }
        #endregion

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            return new RemoteMoveCostCheckStep(activity);
        }
    }
}
