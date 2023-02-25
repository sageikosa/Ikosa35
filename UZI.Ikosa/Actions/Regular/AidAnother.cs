using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    // TODO: consider...AidAnotherAttack, AidAnotherDefense, AidAnotherSkill

    [Serializable]
    public class AidAnother : ActionBase
    {
        public AidAnother(IActionSource source, string orderKey)
            : base(source, new ActionTime( TimeType.Regular), false, false, orderKey)
        {
        }

        public override string Key => @"AidAnother";
        public override string DisplayName(CoreActor actor) => @""; 

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Aid Another", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            throw new NotImplementedException();
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
