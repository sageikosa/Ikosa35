using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class GrabObject : ActionBase
    {
        public GrabObject(CoreObject coreObject, string orderKey)
            : base(coreObject as IActionSource, new ActionTime(TimeType.Brief), new ActionTime(TimeType.Free), true, false, orderKey)
        {
        }

        public CoreObject CoreObject => Source as CoreObject;

        public override string Key
            => @"CoreObject.Grab";

        public override string DisplayName(CoreActor actor)
            => @"Grab object to push, pull or slide";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        /// <summary>Can grab object in-between movement steps</summary>
        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Grab", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(CoreObject, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            if (activity?.Actor.HasActiveAdjunct<ObjectGrabber>() ?? true)
            {
                // cannot grab if currently grabbing (or no activity provided)
                return new ActivityResponse(false);
            }

            // check creature's current action budget; only stackable activity is movement
            if ((activity.Actor.ProcessManager as IkosaProcessManager)?
                .LocalTurnTracker?.GetBudget(activity.Actor.ID) is LocalActionBudget _budget)
            {
                if (!_budget.HasActivity || _budget.TopActivity?.Action is MovementAction)
                {
                    return new ActivityResponse(true);
                }
            }
            return new ActivityResponse(false);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity?.Actor is Creature)
            {
                activity.EnqueueRegisterPreEmptively(Budget);
                return new GrabObjectStep(activity, CoreObject);
            }
            return activity.GetActivityResultNotifyStep(@"Actor not a creature");
        }
    }
}
