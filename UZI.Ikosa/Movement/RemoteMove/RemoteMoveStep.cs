using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class RemoteMoveStep : CoreStep
    {
        protected RemoteMoveStep(CoreActivity activity)
            : base(activity)
        {
        }

        public CoreActivity Activity => Process as CoreActivity;
        public RemoteMoveAction RemoteMoveAction => Activity?.Action as RemoteMoveAction;
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        public StepDestinationTarget StepDestinationTarget
            => Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);

        public ValueTarget<int> StepIndexTarget
            => Activity.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
    }
}
