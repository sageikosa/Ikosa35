using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Unload into the environment [ActionBase (Brief)]
    /// </summary>
    [Serializable]
    public class UnloadObject : ActionBase
    {
        /// <summary>
        /// Unload into the environment [ActionBase (Brief)]
        /// </summary>
        public UnloadObject(IObjectContainer repository, string orderKey)
            : base(repository, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public IObjectContainer Repository => (IObjectContainer)Source;

        public override string Key => @"Inventory.UnloadObject";
        public override string DisplayName(CoreActor actor) => @"Unload object from a container";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Unload", activity.Actor, observer,
                activity.GetFirstTarget<AimTarget>(@"Object").Target as ICoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(Repository as CoreObject, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _locTarget = activity.Targets.OfType<LocationTarget>().FirstOrDefault();
            var _critter = activity.Actor as Creature;
            if ((activity.GetFirstTarget<AimTarget>(@"Object").Target is ICoreObject _target) && (_critter != null) && (_locTarget != null))
            {
                // Need at least 1 free holding slot...
                activity.EnqueueRegisterPreEmptively(Budget);
                var _pickup = new RetrieveObjectStep(activity, Repository, _target);
                new DropStep(_pickup, _critter, _target, _locTarget.Location, true);
                return _pickup;
            }
            return activity.GetActivityResultNotifyStep(@"Invalid targets");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new ObjectListAim(@"Object", @"Object To Unload", FixedRange.One, FixedRange.One, Repository.Objects);
            yield return new LocationAim(@"Location", @"Location to drop item", Visualize.LocationAimMode.Cell,
                FixedRange.One, FixedRange.One, new MeleeRange());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
