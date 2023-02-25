using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class WhirlwindAttack : ActionBase
    {
        public WhirlwindAttack(IActionSource source, Guid id, string orderKey)
            : base(source, new ActionTime(TimeType.Total), false, false, orderKey)
        {
        }

        public override string Key { get { return @""; } }
        public override string DisplayName(CoreActor actor) { return @""; }
        public override string Description { get { return @""; } }
        public override bool CombatList { get { return true; } }

        public override bool IsStackBase(CoreActivity activity)
        {
            return true;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Melee Attack", activity.Actor, observer);
        }

        /// <summary>Overrides default IsHarmless setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless { get { return false; } }

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
