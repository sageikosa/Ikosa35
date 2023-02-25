using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase]</summary>
    [Serializable]
    public class DropHeldObject : ActionBase
    {
        #region construction
        /// <summary>[ActionBase (Free)]</summary>
        public DropHeldObject(HoldingSlot source, string orderKey)
            : base(source, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        /// <summary>[ActionBase] (TimeType)</summary>
        public DropHeldObject(HoldingSlot source, TimeType timeType, string orderKey)
            : base(source, new ActionTime(timeType), false, false, orderKey)
        {
        }
        #endregion

        public HoldingSlot ItemSlot { get { return Source as HoldingSlot; } }
        public override string Key => @"DropObject";
        public override string DisplayName(CoreActor actor)
        {
            if (GetInfoData.GetInfoFeedback(ItemSlot.SlottedItem as CoreObject, actor) is ObjectInfo _info)
            {
                return $@"Drop {_info.Message}";
            }
            if (!string.IsNullOrWhiteSpace(ItemSlot.SubType))
                return $@"Drop Held Object in {ItemSlot.SubType} {ItemSlot.SlotType}";
            else
                return $@"Drop Held Object in {ItemSlot.SlotType}";
        }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => (TimeCost.ActionTimeType == TimeType.Free)
            ? false
            : base.WillClearStack(budget, activity);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Drop", activity.Actor, observer, ItemSlot.SlottedItem as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _gently = ((activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Drop.Gently")) as OptionTarget)?.Key ?? @"Yes") == @"Yes";
            var _position = Locator.FindFirstLocator(ItemSlot.Creature);
            var _location = new CellLocation(_position.Location);
            activity.EnqueueRegisterPreEmptively(Budget);
            return new DropStep(activity, ItemSlot, _location, _gently);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Drop.Gently", @"Drop gently?", true, FixedRange.One, FixedRange.One, GentlyOptions());
            yield break;
        }

        #region private static IEnumerable<OptionAimOption> UseKeyOptions()
        private static IEnumerable<OptionAimOption> GentlyOptions()
        {
            yield return new OptionAimOption() { Key = @"Yes", Name = @"Yes", Description = @"Place object/item gently" };
            yield return new OptionAimOption() { Key = @"No", Name = @"No", Description = @"Drop" };
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
