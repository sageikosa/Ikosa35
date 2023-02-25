using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Note: not used to remove items from holding slot</summary>
    [Serializable]
    public class RemoveSlottedItem : ActionBase
    {
        /// <summary>Note: not used to remove items from holding slot</summary>
        public RemoveSlottedItem(SlottedItemBase slottedItem, string orderKey)
            : base(slottedItem, slottedItem.UnslottingTime, slottedItem.UnslottingProvokes, false, orderKey)
        {
        }

        public SlottedItemBase SlottedItem => ActionSource as SlottedItemBase;

        public override string DisplayName(CoreActor actor)
            => $@"Remove {SlottedItem.GetKnownName(actor)} ";

        public override string Key
            => $@"RemoveSlottedItem";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Removing Item", activity.Actor, observer, SlottedItem);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return new UnslotItemStep(activity, SlottedItem);
        }
    }
}
