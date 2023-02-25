using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class UseSwitch : ActionBase
    {
        public UseSwitch(IActionSource source, SwitchActivationMechanism mechanism, string orderKey)
            : base(source, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
            _Switch = mechanism;
        }

        // TODO: SwitchActivationMechanism instead?
        private SwitchActivationMechanism _Switch;
        public SwitchActivationMechanism Switch => _Switch;

        public override string Key => @"Switch.Use";
        public override string DisplayName(CoreActor actor) => @"Use switch mechanism";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Use switch", activity.Actor, observer, Switch);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // any attempt to use switch is an attempt to reverse current activation state
            var _isClosed = Switch.OpenState.IsClosed;

            // register (might hold process)
            activity.EnqueueRegisterPreEmptively(Budget);

            // after registration, open/close
            var _step = new StartOpenCloseStep(activity, Switch, activity.Actor, this, _isClosed ? 1 : 0);
            _step.AppendFollowing(activity.GetActivityResultNotifyStep(@"Use Switch"));
            _step.AppendFollowing(activity.GetNotifyStep(
                new RefreshNotify(false, true, true, false, false)));
            return _step;
        }
    }
}
