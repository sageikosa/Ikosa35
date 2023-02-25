using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DisableMechanismAction : ActionBase
    {
        public DisableMechanismAction(IDisableable disableable, string orderKey)
            : base(disableable, disableable.ActionTime, true, false, orderKey)
        {
            _Disableable = disableable;
        }

        private IDisableable _Disableable;

        public IDisableable Disableable => _Disableable;

        public override string Key => @"Skill.DisableMechanism";
        public override string DisplayName(CoreActor actor) => @"Disable Mechanism";

        protected override string TimeCostString => Disableable.TimeCostString;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Disable", activity.Actor, observer, Disableable as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.EnqueueRegisterPreEmptively(Budget);
            return new DisableMechanismStep(activity, Disableable);
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
