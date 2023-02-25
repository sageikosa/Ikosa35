using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class InitiativeStartupStep : PreReqListStepBase, ITurnTrackingStep
    {
        #region ctor()
        public InitiativeStartupStep(LocalTurnTracker tracker, IEnumerable<CoreActor> initialActors)
            : base((CoreProcess)null)
        {
            _Tracker = tracker;
            foreach (var _actor in initialActors)
            {
                // initiative values
                _PendingPreRequisites.Enqueue(
                    new RollPrerequisite(this, new Interaction(null, this, _actor, null), _actor,
                        $@"Init.{_actor.ID}", @"Initiative Roll", new DieRoller(20), false));
            }
        }
        #endregion

        private LocalTurnTracker _Tracker;
        public LocalTurnTracker Tracker => _Tracker;

        protected override bool OnDoStep()
        {
            // each initial time is in the midpoint of the slice ...
            // ... so that later additions can be added between, before, and after existing items
            var _count = AllPrerequisites<RollPrerequisite>().Count();
            var _slice = Tracker.TickResolution / _count;
            var _start = Tracker.Map.CurrentTime + _slice / 2;

            // build initial collections (add rolls to initiative, sort)
            foreach (var _order in from _roll in AllPrerequisites<RollPrerequisite>()
                                   let _critter = _roll.Fulfiller as Creature
                                   where (_critter != null)
                                   let _init = _roll.RollValue + _critter.Initiative.EffectiveValue
                                   orderby _init descending
                                   select new { Critter = _critter, Init = _init })
            {
                // all initial creatures are unprepared
                _order.Critter.AddAdjunct(new UnpreparedToDodge(typeof(LocalTurnTracker)));
                _order.Critter.AddAdjunct(new UnpreparedForOpportunities(typeof(LocalTurnTracker)));

                // set time for the tick
                var _tick = LocalTurnTick.CreateInitiativeStartupTick(_start, Tracker, _order.Init);

                // get an action budget associated with the tick
                Tracker.AddBudget(_order.Critter.CreateActionBudget(_tick) as LocalActionBudget);
                _start += _slice;
            }

            // add a round marker
            LocalTurnTick.CreateRoundMarker(Tracker);

            Tracker.CompleteStep(this);
            return true;
        }
    }
}
