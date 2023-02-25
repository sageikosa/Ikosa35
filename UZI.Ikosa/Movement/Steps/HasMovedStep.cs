using Uzi.Core.Contracts;
using System;
using Uzi.Core;
using System.Diagnostics;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class HasMovedStep : MovementProcessStep
    {
        public HasMovedStep(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Set Has Moved";

        protected override bool OnDoStep()
        {
            var _moveAct = MovementAction;
            if (_moveAct != null)
            {
                _moveAct.MovementBudget.HasMoved = true;
            }
            else
            {
                Activity.Terminate(@"Incomplete Activity");
            }
            return true;
        }
    }
}
