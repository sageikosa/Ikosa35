using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// [ActionBase (Free)]
    /// </summary>
    [Serializable]
    public class DropProne : ActionBase
    {
        /// <summary>
        /// [ActionBase (Free)]
        /// </summary>
        public DropProne(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"DropProne";
        public override string DisplayName(CoreActor actor) => @"Drop Prone";

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Drop Prone", activity.Actor, observer);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _prone = new ProneEffect(Source as IActionSource);
            activity.Actor.AddAdjunct(_prone);
            var _register = new RegisterActivityStep(activity, Budget);
            _register.EnqueueNotify(new RefreshNotify(false, true, false, false, false), activity.Actor.ID);
            return _register;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity) { yield break; }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
