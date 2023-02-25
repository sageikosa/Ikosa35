using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Indicates that this creature needs a LocalTurnTick.</summary>
    [Serializable]
    public class NeedsTurnTick : Adjunct
    {
        /// <summary>Indicates that this creature needs a LocalTurnTick.</summary>
        private NeedsTurnTick(LocalTurnTracker tracker)
            : base(tracker)
        {
        }

        public LocalTurnTracker LocalTurnTracker => Source as LocalTurnTracker;

        public override object Clone()
            => new NeedsTurnTick(LocalTurnTracker);

        #region public static bool TryBindToCreature(Creature critter)
        /// <summary>Binds a NeedsTurnTick to a creature if needed and possible.</summary>
        public static bool TryBindToCreature(Creature critter, bool ignoreAwareness = false)
        {
            if (critter?.GetLocalActionBudget() is LocalActionBudget _budget)                   // with an action budget
            {
                if (!_budget.IsInitiative                                                       // budget not in initiative
                    && _budget.TurnTick.TurnTracker.IsInitiative                                // tracker is initiative
                    && !critter.HasAdjunct<NeedsTurnTick>()                                     // not already tagged
                    && (ignoreAwareness || critter.Awarenesses.UnFriendlyAwarenesses.Any()))    // knows unfriendlies are near
                {
                    // signal needs turn tick, associated with a budget that will eject (cleanup) when turn tracker ends
                    _budget.BudgetItems.Add(typeof(NeedsTurnTick),
                        new AdjunctBudget(new NeedsTurnTick(_budget.TurnTick.TurnTracker), @"Needs Turn Tick", @"Signals need to enter turn tick"));
                    return true;
                }
            }
            else
            {
                // not currently in budget
                var _tracker = (critter?.ProcessManager as IkosaProcessManager)?.LocalTurnTracker;
                if ((_tracker?.IsInitiative ?? false)                                               // tracker is initiative
                    && !(_tracker.GetTrackerStep<InitiativeStartupStep>() is InitiativeStartupStep) // tracker is NOT still starting
                    && !critter.HasAdjunct<NeedsTurnTick>()                                         // not already tagged
                    && (ignoreAwareness || critter.Awarenesses.UnFriendlyAwarenesses.Any()))        // knows unfriendlies are near
                {
                    // make an action budget bound to the round marker
                    var _newBudget = critter.CreateActionBudget(_tracker.RoundMarker) as LocalActionBudget;
                    _tracker.AddBudget(_newBudget);

                    // signal needs turn tick, associated with a budget that will eject (cleanup) when turn tracker ends
                    _newBudget.BudgetItems.Add(typeof(NeedsTurnTick),
                        new AdjunctBudget(new NeedsTurnTick(_newBudget.TurnTick.TurnTracker), @"Needs Turn Tick", @"Signals need to enter turn tick"));
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
