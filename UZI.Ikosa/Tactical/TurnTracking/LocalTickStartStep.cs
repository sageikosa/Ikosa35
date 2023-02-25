using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalTickStartStep : CoreStep
    {
        public LocalTickStartStep(CoreStep predecessor, LocalTurnTracker tracker) 
            : base(predecessor)
        {
            _Tracker = tracker;
            _Budget = Tracker.LeadBudgets.FirstOrDefault();
        }

        #region data
        private LocalActionBudget _Budget;
        private LocalTurnTracker _Tracker;
        #endregion

        public LocalTurnTracker Tracker => _Tracker;
        public LocalActionBudget Budget => _Budget;

        public override bool IsDispensingPrerequisites => false;

        protected override bool OnDoStep()
        {
            Tracker.StartOfTick();

            // critter is not longer unprepared once it can act
            if (_Budget != null)
            {
                if (_Budget.Actor is Creature _critter)
                {
                    _critter.Adjuncts.OfType<UnpreparedToDodge>()
                        .FirstOrDefault(_d => (_d.Source as Type) == typeof(LocalTurnTracker))?.Eject();
                    _critter.Adjuncts.OfType<UnpreparedForOpportunities>()
                        .FirstOrDefault(_d => (_d.Source as Type) == typeof(LocalTurnTracker))?.Eject();
                }
            }
            new LocalTickStep(this, Tracker);
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;
    }
}
