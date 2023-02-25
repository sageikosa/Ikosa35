using System;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ForceDoubleStep : MovementProcessStep
    {
        public ForceDoubleStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name { get { return @"Force double move"; } }

        protected override bool OnDoStep()
        {
            this.MovementAction.MovementRangeBudget.ForceDouble();
            return true;
        }
    }
}
