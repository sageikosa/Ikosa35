using System;
using System.Diagnostics;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RegisterOverrun : MovementProcessStep
    {
        public RegisterOverrun(CoreActivity activity)
            : base(activity)
        {
        }

        public override string Name => @"Register Overrun";

        protected override bool OnDoStep()
        {
            MovementAction?.Budget?.BudgetItems.Add(typeof(OverrunBudget), new OverrunBudget());
            return true;
        }
    }
}
