using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class EndRage : ActionBase
    {
        public EndRage(IActionSource powerClass, string orderKey)
            : base(powerClass, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
        }
        public override string Key => @"Rage.End";
        public override string DisplayName(CoreActor actor) => @"Stop Raging";
        public override bool IsMental => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Calming down", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.Actor?.Adjuncts.OfType<Raging>().FirstOrDefault(_r => _r.Source == ActionSource)?.Eject();
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
