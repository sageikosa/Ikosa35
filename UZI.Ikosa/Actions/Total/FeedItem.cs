using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class FeedItem : ActionBase
    {
        /// <summary>Administer potion to creature [ActionBase (Regular)]</summary>
        public FeedItem(IFeedableItem feedableItem, string orderKey)
            : base(feedableItem, new ActionTime(TimeType.Total), true, true, orderKey)
        {
        }

        public IFeedableItem FeedableItem => (IFeedableItem)Source;

        public override string Key => @"Item.Administer";
        public override string DisplayName(CoreActor actor) => $@"Administer: {FeedableItem.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Administer", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(FeedableItem as CoreObject, observer);
            return _obs;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return FeedableItem.DoFeed(activity);
        }
        #endregion

        /// <summary>Returns the SpellMode's aiming mode, except it switches creature target range to melee</summary>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => FeedableItem.FeedableAimingMode(activity);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
        {
            if (ProvokesTarget)
            {
                var _loc = potentialTarget?.GetLocated()?.Locator;
                if (_loc != null)
                {
                    foreach (var _target in activity.Targets.OfType<AttackTarget>())
                    {
                        if (_target.Target.ID == potentialTarget.ID)
                            return true;

                        if ((_target.Target as IAdjunctable)?.GetLocated()?.Locator == _loc)
                            return true;
                    }
                }
            }
            return false;
        }
    }

    public interface IFeedableItem : IActionSource, IItemBase
    {
        CoreStep DoFeed(CoreActivity activity);
        IEnumerable<AimingMode> FeedableAimingMode(CoreActivity activity);
    }
}
