using Uzi.Core;

namespace Uzi.Ikosa.Universal
{
    /// <summary>Used for budget items that reset with the action budget itself</summary>
    public interface IResetBudgetItem : IBudgetItem
    {
        /// <summary>Perform whatever reset is necessary.  Returns true if the budget item needs to be removed when done</summary>
        bool Reset();
    }
}
