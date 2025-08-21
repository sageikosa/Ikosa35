using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class TumbleBudget : ITurnEndBudgetItem, IInteractHandler
    {
        // TODO: consider making protected
        // TODO: and having a static factory method (like FlightBudget)
        public TumbleBudget(Creature critter)
        {
            _Critter = critter;
            _Past = [];
            _Thru = [];
        }

        // TODO: accelerated switch here, or on action?

        #region private data
        private Creature _Critter;
        private List<Guid> _Past;
        private List<Guid> _Thru;
        #endregion

        public Creature Creature => _Critter;

        /// <summary>NOTE: does not account for accelerated, nor surface conditions</summary>
        public int TumblePastBase
            => 15 + (_Past.Count * 2);

        /// <summary>NOTE: does not account for accelerated, nor surface conditions</summary>
        public int TumbleThruBase
            => 25 + (_Thru.Count * 2);

        #region interface implementations

        public bool EndTurn() => true;
        public string Name => @"Tumble Budget";
        public string Description => @"Tracks creatures tumbling past or through";
        public void Added(CoreActionBudget budget) => Creature?.AddIInteractHandler(this);
        public void Removed() => Creature?.RemoveIInteractHandler(this);
        public object Source => typeof(TumbleBudget);

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            // probably need provoke data, so explicitly store and check the cast
            var _provoke = workSet?.InteractData as OpportunisticProvokeData;
            if (_provoke != null)
            {
                // must be a tumble move...
                var _tumble = _provoke?.Activity?.Action as Tumble;
                if (_tumble != null)
                {
                    var _feedback = new OpportunisticProvokeFeedback(this);
                    var _id = _provoke.Attacker.ID;
                    if (_Thru.Contains(_id))
                    {
                        // already proved we can tumble through the attacker
                        _feedback.SuccessCheck =
                            new PreviousSuccess(this, @"Tumble", @"Tumbling Past", true);
                    }
                    else
                    {
                        // NOTE: every time a tumble check is made, the difficulty goes up for the next one...

                        // tumbling into the attacker?
                        var _loc = Creature?.GetLocated()?.Locator;
                        var _region = _provoke?.Activity.GetFirstTarget<ValueTarget<IGeometricRegion>>(@"TargetRegion");
                        if (_loc?.MapContext.LocatorsInRegion(_region?.Value, _loc?.PlanarPresence ?? PlanarPresence.Material)
                            .Any(_l => _l.Chief?.ID == _id) ?? false)
                        {
                            var _check = new SuccessCheck(Creature.Skills.Skill<TumbleSkill>(), TumbleThruBase, _provoke.Activity, -10);
                            var _qualifier = new Qualifier(Creature, _provoke.Activity, _provoke.Attacker);
                            _feedback.SuccessCheck = new SuccessCheckPrerequisite(this, _qualifier,
                                @"Tumble", @"Tumbling Thru", _check, false);
                            _Thru.Add(_id);
                        }
                        else if (_Past.Contains(_id))
                        {
                            // already tried tumbling past the attacker
                            // TODO: attacker only gets to choose on first attempt to tumble past?
                            _feedback.SuccessCheck =
                                new PreviousSuccess(this, @"Tumble", @"Tumbling Past", true);
                        }
                        else
                        {
                            var _check = new SuccessCheck(Creature.Skills.Skill<TumbleSkill>(), TumblePastBase, _provoke.Activity, -10);
                            var _qualifier = new Qualifier(Creature, _provoke.Activity, _provoke.Attacker);
                            _feedback.SuccessCheck = new SuccessCheckPrerequisite(this, _qualifier,
                                @"Tumble", @"Tumbling Past", _check, false);
                            _Past.Add(_id);
                        }
                    }
                    workSet.Feedback.Add(_feedback);
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(OpportunisticProvokeData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler) => true;

        #endregion
    }
}
