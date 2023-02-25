using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>May provide actions when an actor is within melee range</summary>
    public interface ITacticalActionProvider : IActionProvider
    {
        /// <summary>Tactical actions must not account for any actions provided by ITacticalActionProvider sub-parts</summary>
        IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget);
        bool IsContextMenuOnly { get; }
    }

    public static class TacticalActionHelper
    {
        #region public static IEnumerable<CoreAction> TacticalActions(this ICoreObject container, LocalActionBudget budget)
        /// <summary>Yields actions for all for nested accessible ITacticalActionProviders on the container</summary>
        public static IEnumerable<CoreAction> AccessibleActions(this ICoreObject container, LocalActionBudget budget)
        {
            Creature _critter = budget.Actor as Creature;
            return from _tact in container.AllAccessible(null, budget.Actor).OfType<ITacticalActionProvider>()
                   where (_critter?.Awarenesses.GetAwarenessLevel(_tact.ID) == AwarenessLevel.Aware)
                   from _act in _tact.GetTacticalActions(budget)
                   select _act;
        }
        #endregion
    }
}
