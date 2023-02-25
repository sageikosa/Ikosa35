using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class Upkeep : ActionBase
    {
        public Upkeep(IActionSource source, ActionTime actionTime, string orderKey)
            : base(source, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.Upkeep";
        public override string DisplayName(CoreActor actor) => @"Party and Personal Upkeep";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => true;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            return null;
        }
    }
}
