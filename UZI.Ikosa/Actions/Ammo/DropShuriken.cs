using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DropShuriken : ActionBase
    {
        public DropShuriken(ShurikenGrip grip, string orderKey)
            : base(grip, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
        }

        public ShurikenGrip ShurikenGrip => ActionSource as ShurikenGrip;

        public override string Key => @"Shuriken.Drop";
        public override string DisplayName(CoreActor actor) => @"Drop held shuriken";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Drop", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(ShurikenGrip.Ammunition[0], observer);
            return _obs;
        }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            ShurikenGrip.ClearSlots();
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
