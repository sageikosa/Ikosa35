using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// ActionBase that always registers budget use pre-emptively, and returns a SimpleStep with a NotifyStep follower
    /// </summary>
    [Serializable]
    public abstract class SimpleActionBase : ActionBase, ISimpleStep
    {
        /// <summary>
        /// ActionBase that always registers budget use pre-emptively, and returns a SimpleStep with a NotifyStep follower
        /// </summary>
        protected SimpleActionBase(IActionSource actionSource, ActionTime timeCost, 
            bool provokesMelee, bool provokesTarget, string orderKey)
            : base(actionSource, timeCost, provokesMelee, provokesTarget, orderKey)
        {
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            var _step = new SimpleStep(activity, this);
            _step.AppendFollowing(OnSuccessNotify(activity));
            return _step;
        }

        protected abstract NotifyStep OnSuccessNotify(CoreActivity activity);

        public abstract bool DoStep(CoreStep actualStep);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => null;
    }
}
