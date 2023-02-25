using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class OverrunningBudget : ITurnEndBudgetItem
    {
        public OverrunningBudget(Creature target)
        {
            _Target = target;
        }

        #region data
        private Creature _Target;
        #endregion

        public Creature Target => _Target;

        // ITurnEndBudgetItem Members
        public bool EndTurn()
            => true;

        // IBudgetItem Members
        public string Name => @"Overrun Attempt Budget";
        public string Description => @"Tracks overrun attempts (1 allowed)";

        public void Added(CoreActionBudget budget)
        {
        }

        public void Removed()
        {
        }

        // ISourcedObject Members
        public object Source
            => typeof(OverrunningBudget);
    }
}
