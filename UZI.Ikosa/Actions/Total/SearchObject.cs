using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SearchObject : ActionBase
    {
        public SearchObject(IActionSource powerLevel, ActionTime cost, string orderKey)
            : base(powerLevel, cost, true, true, orderKey)
        {
        }

        public override string Key => @"Search.Object";
        public override string DisplayName(CoreActor actor) => @"Search Object";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // TODO: object aim
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Search", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false; // TODO: ???

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            // TODO:
            return null;
        }
    }
}
