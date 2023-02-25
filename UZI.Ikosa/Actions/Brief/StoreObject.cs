using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// store object being held [ActionBase (Brief)]
    /// </summary>
    [Serializable]
    public class StoreObject : ActionBase
    {
        /// <summary>
        /// creature must be holding something
        /// </summary>
        public static bool CanStore(IObjectContainer repository, Creature actor)
        {
            // creature must be holding something
            return (from _slot in actor.Body.ItemSlots.AllSlots
                    where _slot.SlotType.Equals(ItemSlot.HoldingSlot)
                    && (_slot.SlottedItem != null)
                    select _slot).Count() > 0;
        }

        /// <summary>
        /// store object being held [ActionBase (Brief)]
        /// </summary>
        public StoreObject(IObjectContainer repository, string orderKey)
            : base(repository, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public IObjectContainer Repository
            => (IObjectContainer)Source;

        public override string Key => @"Inventory.StoreObject";
        public override string DisplayName(CoreActor actor) => @"Store object being held";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Store", activity.Actor, observer,
                activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(Repository as CoreObject, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _item = (activity.Actor as Creature)?.Body.ItemSlots.
                SlotForItem((activity.Targets.OfType<AimTarget>()?.FirstOrDefault())?.Target as ICoreObject)?.SlottedItem;
            activity.EnqueueRegisterPreEmptively(Budget);
            return new StoreObjectStep(activity, Repository, _item.MainSlot as HoldingSlot);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new ObjectListAim(@"Item", @"Items in hand", FixedRange.One, FixedRange.One, Items(activity));
            yield break;
        }

        private IEnumerable<ICoreObject> Items(CoreActivity activity)
        {
            // object being held
            var _critter = activity.Actor as Creature;
            foreach (var _slot in _critter.Body.ItemSlots.AllSlots)
            {
                if (_slot.SlotType.Equals(ItemSlot.HoldingSlot) && (_slot.SlottedItem != null))
                {
                    yield return _slot.SlottedItem.BaseObject;
                }
            }
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
