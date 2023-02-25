using Uzi.Core.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Single target held charge [ActionBase (Regular)]
    /// </summary>
    [Serializable]
    public class DeliverHeldCharge : ActionBase
    {
        /// <summary>
        /// Single target held charge [ActionBase (Regular)]
        /// </summary>
        public DeliverHeldCharge(HeldCharge charge, string orderKey)
            : base(charge, new ActionTime(TimeType.Regular), false, charge.CastSpell.SpellMode.IsHarmless, orderKey)
        {
        }

        public HeldCharge HeldCharge => Source as HeldCharge;
        public override string Key => @"Touch.HeldCharge";

        public override string DisplayName(CoreActor actor)
            => $@"Deliver Held Charge: {HeldCharge.CastSpell.PowerActionSource.SpellDef.DisplayName}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Touch", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // stitch up old delivery with targets from new activity...
            foreach (AimTarget _target in activity.Targets)
            {
                for (int _tx = 0; _tx < HeldCharge.OriginalActivity.Targets.Count; _tx++)
                {
                    if (HeldCharge.OriginalActivity.Targets[_tx] == null)
                    {
                        HeldCharge.OriginalActivity.Targets[_tx] = _target;
                        break;
                    }
                }
            }

            // once the action is performed, removed the held charge
            // NOTE: if it needs to get re-established, the next HoldChargeStep will create a new one
            HeldCharge.ClearSlots();
            HeldCharge.Possessor = null;

            // perform the original action with the new touch targets...disable the current activity...
            HeldCharge.OriginalActivity.IsActive = true;
            var _located = activity.Actor.GetLocated();
            if (_located != null)
                _located.Locator.MapContext.ContextSet.ProcessManager.StartProcess(HeldCharge.OriginalActivity);
            activity.IsActive = false;

            // no root step...
            return null;
        }

        /// <summary>
        /// Reselect all touch targets
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _mode in HeldCharge.CastSpell.SpellMode.AimingMode(activity.Actor, HeldCharge.CastSpell.SpellMode))
            {
                var _touch = _mode as TouchAim;
                if (_touch?.Range is MeleeRange)
                    yield return _touch;
            }
            yield break;
        }

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
}
