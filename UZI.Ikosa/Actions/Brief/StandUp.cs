using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase (Brief)]</summary>
    [Serializable]
    public class StandUp : ActionBase
    {
        /// <summary>[ActionBase (Brief)]</summary>
        public StandUp(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public override string Key => @"Movement.StandUp";
        public override string DisplayName(CoreActor actor) => @"Stand Up";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Stand Up", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            foreach (var _prone in activity.Actor.Adjuncts.OfType<ProneEffect>().ToList())
            {
                _prone.Eject();
            }
            var _register = new RegisterActivityStep(activity, Budget);
            _register.EnqueueNotify(new RefreshNotify(false, true, false, false, false), activity.Actor.ID);
            return _register;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity) { yield break; }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
