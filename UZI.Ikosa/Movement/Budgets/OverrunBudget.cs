using System;
using Uzi.Core;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class OverrunBudget : ITurnEndBudgetItem
    {
        public OverrunBudget()
        {
        }

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
            => typeof(OverrunBudget);
    }
}
