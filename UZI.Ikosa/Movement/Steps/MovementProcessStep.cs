using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class MovementProcessStep : CoreStep
    {
        protected MovementProcessStep(CoreActivity activity)
            : base(activity)
        {
        }

        public CoreActivity Activity => Process as CoreActivity;
        public MovementAction MovementAction => Activity?.Action as MovementAction;
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        public StepDestinationTarget StepDestinationTarget
            => Activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);

        public ValueTarget<int> StepIndexTarget
            => Activity.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
    }
}
