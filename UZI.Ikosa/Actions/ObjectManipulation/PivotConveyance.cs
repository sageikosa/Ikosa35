using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PivotConveyance : ActionBase
    {
        public PivotConveyance(Conveyance conveyance, ActionTime time)
            : base(conveyance, time, true, false, @"202")
        {
            if (time.ActionTimeType == TimeType.Brief)
            {
                _TurnsLeft = ObjectGrabbedPivotData.GetPivots(conveyance) * 2;
            }
            else
            {
                _TurnsLeft = 0;
            }
        }

        #region data
        private int _TurnsLeft;
        #endregion

        public Conveyance Conveyance => Source as Conveyance;
        public int TurnsLeft { get => _TurnsLeft; set => _TurnsLeft = value; }

        public override string Key
            => @"Conveyance.Pivot";

        public override string DisplayName(CoreActor actor)
            => @"Pivot conveyance";

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // first check effort budget
            var _response = base.CanPerformNow(budget);
            if (!_response.Success)
                return _response;

            // see if the turns are still left

            // if this is stacked, then check the stack base
            if (Budget?.HasActivity ?? false)
            {
                return new ActivityResponse(((Budget?.TopActivity?.Action as PivotConveyance) ?? this).TurnsLeft > 0);
            }

            // no, then this is the main constraining action
            return new ActivityResponse(TurnsLeft > 0);
        }
        #endregion

        /// <summary>Actions can stack if this is not a sub-action</summary>
        public override bool IsStackBase(CoreActivity activity)
            => (TimeCost.ActionTimeType != TimeType.SubAction) && (_TurnsLeft > 0);

        /// <summary>Pops actions from stack if this is not a sub-action</summary>
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => TimeCost.ActionTimeType != TimeType.SubAction;

        public override string HeadsUpMode => @"Conveyance";

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Pivot", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(Conveyance, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new RotateAim(@"Pivot", $@"Pivot {Conveyance.GetInfo(activity.Actor, false)}");
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // decrease turns left
            var _pivot = ((Budget?.TopActivity?.Action as PivotConveyance) ?? this);
            _pivot.TurnsLeft--;
            if (_pivot.TurnsLeft <= 0)
            {
                // clear activity stack when pivot cannot turn anymore
                Budget?.ClearActivities();
            }

            activity?.EnqueueRegisterPreEmptively(Budget);

            // perform rotation
            return new HandleInteractionStep(activity, Conveyance,
                new Interaction(activity.Actor, this, Conveyance,
                new ConveyanceRotateData(activity.Actor,
                activity.GetFirstTarget<CharacterStringTarget>(@"Pivot").CharacterString)));
        }
    }
}
