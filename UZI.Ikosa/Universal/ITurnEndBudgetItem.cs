using Uzi.Core;

namespace Uzi.Ikosa.Universal
{
    /// <summary>Used for budget items that need to modify themselves when the turn ends</summary>
    public interface ITurnEndBudgetItem : IBudgetItem
    {
        /// <summary>Returns true if the budget item needs to be removed when done</summary>
        bool EndTurn();
    }
}
