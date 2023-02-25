using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ConsumeItem : ActionBase
    {
        /// <summary>Self-Drink Item [ActionBase (Regular)]</summary>
        public ConsumeItem(IConsumableItem consumable, string orderKey)
            : base(consumable, new ActionTime(TimeType.Regular), true, false, orderKey)
        {
        }

        public IConsumableItem Consumable => (IConsumableItem)Source;

        public override string Key => @"Item.Drink";
        public override string DisplayName(CoreActor actor) => $@"Drink: {Consumable.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Consume", activity.Actor, observer, Consumable as CoreObject);
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            activity.AppendCompletion(
                activity.GetActivityResultNotifyStep(@"Consumed"),
                activity.GetNotifyStep(new RefreshNotify(true, false, false, true, false)));
            return Consumable.DoConsume(activity);
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        /// <summary>Returns the SpellMode's aiming mode, except it replaces any creature target with the actor</summary>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            return Consumable.ConsumptionAimingMode(activity);
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    public interface IConsumableItem : IItemBase, IActionSource
    {
        CoreStep DoConsume(CoreActivity activity);
        IEnumerable<AimingMode> ConsumptionAimingMode(CoreActivity activity);
    }
}
