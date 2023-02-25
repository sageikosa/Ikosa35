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
    public class EnableMechanismAction : ActionBase
    {
        public EnableMechanismAction(IDisableable disableable, string orderKey)
            : base(disableable, disableable.ActionTime, true, false, orderKey)
        {
            _Disableable = disableable;
        }

        private IDisableable _Disableable;

        public IDisableable Disableable => _Disableable;

        public override string Key => @"Skill.EnableMechanism";
        public override string DisplayName(CoreActor actor) => @"Enable Mechanism";

        protected override string TimeCostString => Disableable.TimeCostString;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Enable", activity.Actor, observer, Disableable as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.EnqueueRegisterPreEmptively(Budget);
            return new EnableMechanismStep(activity, Disableable);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // target is supplied by the IDisableable itself
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
