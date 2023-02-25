using Uzi.Core;

namespace Uzi.Ikosa.Universal
{
    /// <summary>Used for budget items that need to do something when the actor's tick ends</summary>
    public interface ITickEndBudgetItem : IBudgetItem
    {
        /// <summary>Returns true if the budget item needs to be removed when done</summary>
        bool EndTick();
    }
}
