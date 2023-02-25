using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RegisterOverrunning : MovementProcessStep
    {
        public RegisterOverrunning(CoreActivity activity, Creature target)
            : base(activity)
        {
            _Critter = target;
        }

        #region data
        private Creature _Critter;
        #endregion

        public override string Name => @"Register Overrunning";

        protected override bool OnDoStep()
        {
            MovementAction?.Budget?.BudgetItems.Add(typeof(OverrunningBudget), new OverrunningBudget(_Critter));
            return true;
        }
    }
}
