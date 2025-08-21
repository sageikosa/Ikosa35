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
    public class ApplyItem : ActionBase
    {
        /// <summary>Apply oil to whatever [ActionBase (Regular)]</summary>
        public ApplyItem(IAppliableItem appliable, string orderKey)
            : base(appliable, new ActionTime(TimeType.Regular), true, true, orderKey)
        {
        }

        public IAppliableItem Appliable => (IAppliableItem)Source;

        public override string Key => @"Item.Apply";
        public override string DisplayName(CoreActor actor) => $@"Apply: {Appliable.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Apply", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(Appliable as CoreObject, observer);
            return _obs;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return Appliable.DoApply(activity);
        }
        #endregion

        /// <summary>Returns the SpellMode's aiming mode, except it switches object target range to melee</summary>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            =>  Appliable.ApplicationAimingMode(activity);

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
                        {
                            return true;
                        }

                        if ((_target.Target as IAdjunctable)?.GetLocated()?.Locator == _loc)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public interface IAppliableItem : IItemBase, IActionSource
    {
        CoreStep DoApply(CoreActivity activity);
        IEnumerable<AimingMode> ApplicationAimingMode(CoreActivity activity);
    }
}
