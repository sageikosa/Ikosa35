using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// retrieve object to holding/wielding [ActionBase] (Brief)]
    /// </summary>
    [Serializable]
    public class RetrieveObject : ActionBase
    {
        /// <summary>
        /// critter must have a free holding slot
        /// </summary>
        public static bool CanRetrieve(IObjectContainer repository, Creature actor)
        {
            // to retrieve something, critter must have a free holding slot
            return (from _slot in actor.Body.ItemSlots.AllSlots
                    where _slot.SlotType.Equals(ItemSlot.HoldingSlot)
                    && (_slot.SlottedItem == null)
                    select _slot).Count() > 0;
        }

        /// <summary>
        /// retrieve object to holding/wielding [ActionBase (Brief)]
        /// </summary>
        public RetrieveObject(IObjectContainer repository, string orderKey)
            : base(repository, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public IObjectContainer Repository => (IObjectContainer)Source;
        public override string Key => @"Inventory.RetrieveObject";
        public override string DisplayName(CoreActor actor) => @"Retrieve object to holding";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Retrieve", activity.Actor, observer,
                activity.GetFirstTarget<AimTarget>(@"Object")?.Target as ICoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(Repository as CoreObject, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((activity.Actor is Creature _critter)
                && (activity.GetFirstTarget<AimTarget>(@"Object")?.Target is ICoreObject _obj))
            {
                activity.EnqueueRegisterPreEmptively(Budget);
                return new RetrieveObjectStep(activity, Repository, _obj);
            }
            return activity.GetActivityResultNotifyStep(@"Invalid creature or object");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new ObjectListAim(@"Object", @"Object To Retrieve", FixedRange.One, FixedRange.One, Repository.Objects);
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
