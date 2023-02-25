using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    public class TotalDefense : ActionBase
    {
        // NOTE: no interaction needed
        // TODO: UnpreparedForReaction while defending this way
        // TODO: +4 Dodge for 1 Round
        public TotalDefense(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
        }

        public override string Key =>@"Action.TotalDefense"; 
        public override string DisplayName(CoreActor actor) => @"Total Defense";
        public override bool CombatList => true;

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Total Defense", activity.Actor, observer);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO:
            throw new Exception("The method or operation is not implemented.");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            throw new NotImplementedException();
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}