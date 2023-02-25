using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>determines whether each potential opportunistic attacker can make an attack, and if so, will make an attack</summary>
    [Serializable]
    public class OpportunisticInquiry : CoreStep
    {
        #region construction
        /// <summary>determines whether each potential opportunistic attacker can make an attack, and if so, will make an attack</summary>
        public OpportunisticInquiry(CoreActivity activity)
            : base((CoreProcess)activity)
        {
            // capture action budget order
            _BudgetOrder = GetBudgetCaptures(_Activity.Actor as Creature).ToList();
            // TODO: get viable attack targets from the opportunity.Action (may be different than actor)
        }
        #endregion

        #region data
        private CoreActivity _Activity
            => Process as CoreActivity;

        private List<BudgetCapture> _BudgetOrder;

        private class BudgetCapture
        {
            public LocalActionBudget Budget { get; set; }
            public IEnumerable<StrikeCapture> Captures { get; set; }
        }
        #endregion

        private static IEnumerable<BudgetCapture> GetBudgetCaptures(Creature target)
        {
            var _locator = target?.GetLocated()?.Locator;
            if (_locator != null)
            {
                var _geom = _locator.GeometricRegion;
                var _map = _locator.Map;
                return (from _budgt in _locator.IkosaProcessManager.LocalTurnTracker.ReactableBudgets
                        let _critter = _budgt.Actor as Creature
                        let _wieldLoc = _critter?.GetLocated()?.Locator
                        where _wieldLoc != null
                        // budget holder must be aware of the target
                        && (_critter.Awarenesses.GetAwarenessLevel(target.ID) >= AwarenessLevel.Aware)
                        // ... and must be able to make an opportunistic attack
                        && !_critter.Conditions.Contains(Condition.UnpreparedForOpportunities)
                        // ... and cannot be the target
                        && _critter != target
                        // ... and cannot have hard cover
                        && !_locator.HasMeleeCover(_wieldLoc)
                        // ... joined with their strike zones that can make opportunistic attacks
                        join _capture in _map.MapContext.StrikeZones.AllCaptures
                            .Where(_c => _c.ZoneLink.StrikeZoneMaster.Weapon.OpportunisticAttacks
                            // ... as long as the locator of the actor is in the strike zone region
                            && _geom.ContainsGeometricRegion(_c.Geometry.Region))
                        on _critter equals _capture.ZoneLink.StrikeZoneMaster.Weapon.Possessor
                        into _captures
                        where _captures.Any()
                        select new BudgetCapture { Budget = _budgt, Captures = _captures });
            }
            return new BudgetCapture[] { };
        }

        /// <summary>Get creatures that threaten (aware of target, preprared for opportunities, no hard cover, and wielding a weapon that can make attacks)</summary>
        /// <param name="creature"></param>
        public static IEnumerable<Creature> GetThreateningCreatures(Creature creature)
            => GetBudgetCaptures(creature).Select(_bc => _bc.Budget.Creature);

        /// <summary>Activity that caused the potential opportunistic attack</summary>
        public CoreActivity Activity => _Activity;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // if the activity is no longer active, no need to do anything
            if (!_Activity.IsActive)
            {
                return true;
            }

            // get locator of the activity's actor
            var _locator = Locator.FindFirstLocator(_Activity.Actor);

            OpportunisticAttackCheck _yieldOne(BudgetCapture oppty)
            {
                var _repetoire = oppty.Budget.Actor.Actions;
                var _critter = _repetoire.Actor as Creature;
                if ((_critter?.IgnoreFriendlyOpportunities ?? false)
                    && (_critter?.IsFriendly(_Activity.Actor.ID) ?? false))
                {
                    return null;
                }

                // see if the budget has opportunistic budget left (and hasn't attacked against this opportunity)
                var _opptyBudget = oppty.Budget.BudgetItems[typeof(OpportunityBudget)] as OpportunityBudget;
                if (_opptyBudget.CanTakeOpportunity(_Activity))
                {

                    // see if wielded weapons can make an attack on the target
                    // list of the possible opportunistic attacks
                    var _atks = (from _capt in oppty.Captures
                                 let _wpn = _capt.ZoneLink.StrikeZoneMaster.Weapon
                                 from _strike in _wpn.WeaponStrikes()
                                 let _oAtk = new OpportunisticAttack(_strike, _Activity, _strike.OrderKey)
                                 where _oAtk.CanPerformNow(oppty.Budget).Success
                                 && !_repetoire.SuppressAction(oppty.Budget, _Activity, _oAtk)
                                 select (_wpn as IActionProvider, _oAtk))
                                 .ToList();
                    if (_atks.Any())
                    {
                        return new OpportunisticAttackCheck(this, oppty.Budget.Actor, _atks);
                    }
                }
                return null;
            }

            // simple enough to do same thing multi-next step does...
            foreach (var _next in from _bc in _BudgetOrder
                                  let _step = _yieldOne(_bc)
                                  where _step != null
                                  select _step)
            {
                // NOTE: OpportunisticInquiry is enqueued reactively
                //       so these are also processed reactively by the ProcessManager
                AppendFollowing(_next);
            }
            return true;
        }
        #endregion
    }
}
