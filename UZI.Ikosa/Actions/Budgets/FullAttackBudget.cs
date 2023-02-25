using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Designates weapon use in full attack sequences, initialized after first attack is complete.
    /// Called after each subsequent attack in a finisher sequence.
    /// </summary>
    [Serializable]
    public class FullAttackBudget : ITurnEndBudgetItem, IActionProvider, IQualifyDelta
    {
        #region construction
        public FullAttackBudget(AttackActionBase attack)
        {
            _Attack = attack;
            _Budget = null;
            _Term = new TerminateController(this);
        }
        #endregion

        #region Temporary Secondary Natural Weapon Setup
        /// <summary>Temporarily makes each primary natural weapon secondary a secondary one</summary>
        private void TemporarySecondaries()
        {
            foreach (var _natWpn in from _n in Body.NaturalWeapons
                                    where _n.IsPrimary
                                    select _n)
            {
                _natWpn.AddAdjunct(new NaturalSecondaryAdjunct(typeof(FullAttackBudget)));
            }
        }

        private void CleanupSecondaries()
        {
            // remove all secondaries sourced from the full attack budget
            foreach (var _secondary in (from _n in Body.NaturalWeapons
                                        from _s in _n.Adjuncts.OfType<NaturalSecondaryAdjunct>()
                                        where _s.Source.Equals(typeof(FullAttackBudget))
                                        select _s).ToList())
            {
                _secondary.Eject();
            }
        }
        #endregion

        #region data
        private AttackActionBase _Attack;
        private CoreActionBudget _Budget;
        private readonly TerminateController _Term;
        private Guid _ID = Guid.NewGuid();
        #endregion

        public LocalActionBudget Budget => _Budget as LocalActionBudget;
        public AttackActionBase PrimaryAttack => _Attack;

        protected Creature _Creature => _Budget.Actor as Creature;

        public Body Body => _Creature.Body;
        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        /// <summary>True if some potential has been used to make an attack</summary>
        public bool AttackStarted
            => _Budget.BudgetItems.Items.OfType<IAttackPotential>().Any(_ap => _ap.IsUsed);

        #region public void EndTurn()
        /// <summary>called at the end of the turn</summary>
        public bool EndTurn()
        {
            // must remove budget item
            return true;
        }
        #endregion

        #region public void UseWeaponHead(WeaponBase weapon, WeaponHead head)
        /// <summary>
        /// Only call for an unused natural weapon, a manufactured weapon in budget, or when no primary weapon has been used yet.
        /// Do not use until after cleave is resolved.
        /// </summary>
        public void UseAttack(AttackActionBase attack)
        {
            // find first potential
            var _primary = _Budget.BudgetItems.Items.OfType<IAttackPotential>()
                .OrderBy(_ap => _ap is ISlotAttackPotential ? 2 : 1)
                .FirstOrDefault(_ap => _ap.CanUse(attack));
            if (_primary != null)
            {
                if (_primary is ISlotAttackPotential)
                {
                    // if a slot potential, register all slots involved as well (such as by assigned slots)
                    foreach (var _slots in _Budget.BudgetItems.Items.OfType<IAttackPotential>()
                        .Where(_ap => _ap.CanUse(attack))
                        .ToList())
                        _slots.RegisterUse(attack);
                }
                else
                {
                    // not a slot potential, so just use it
                    _primary.RegisterUse(attack);
                }
            }
        }
        #endregion

        // ISourcedObject
        public object Source => typeof(AttackActionBase);

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // must be an interaction
            if (qualify is Interaction _iAct)
            {
                // must be an attack
                if (_iAct.InteractData is AttackData _atk)
                {
                    // must have an attack action underneath
                    if (_atk.Action is ISupplyAttackAction _supplier)
                    {
                        // must have a "current" potential
                        var _potential = _Budget.BudgetItems.Items.OfType<IAttackPotential>()
                            .OrderBy(_ap => _ap is ISlotAttackPotential ? 2 : 1)
                            .FirstOrDefault(_ap => _ap.CanUse(_supplier.Attack));
                        if (_potential != null)
                            yield return _potential.Delta;
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            // allow natural weapons to resume normal primacy settings
            CleanupSecondaries();

            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        /// <summary>Gets any defined full attack budget</summary>
        public static FullAttackBudget GetBudget(CoreActionBudget budget)
        {
            if (budget.BudgetItems.ContainsKey(typeof(AttackActionBase)))
                return budget.BudgetItems[typeof(AttackActionBase)] as FullAttackBudget;
            return null;
        }

        #region IBudgetItem Members

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
            var _critter = _Creature;
            if (_critter != null)
            {
                _critter.Actions.Providers.Add(this, this);

                // adds passthrough penalties from "current" potential
                _critter.MeleeDeltable.Deltas.Add(this);
                _critter.RangedDeltable.Deltas.Add(this);
                _critter.OpposedDeltable.Deltas.Add(this);
            }

            if (_critter.Conditions.Contains(Condition.Grappling))
            {
                // TODO: grapple potential...(if first attack made, can a weapon still be used...?)
            }
            else
            {
                if (!(PrimaryAttack.Weapon is NaturalWeapon))
                {
                    // all natural weapons will be secondaries
                    TemporarySecondaries();

                    // main attack sequence allowed
                    var _slot = PrimaryAttack.WeaponHead?.AssignedSlots.FirstOrDefault();
                    if (_slot != null)
                    {
                        // attack was with a weapon (as opposed to a grapple...)
                        var _main = new SequencePotential(_slot, _Creature.BaseAttack.FullAttackCount);
                        Budget.BudgetItems.Add(_main.Source, _main);

                        // off-hand (and non-primary unarmed) slots marked
                        foreach (var _off in from _s in Body.ItemSlots.AllSlots
                                             where (_s != _slot)
                                             && ((_s is HoldingSlot) || (_s.SlotType == ItemSlot.UnarmedSlot))
                                             select _s)
                            _Creature.AddAdjunct(new OffHand(this, _off));
                    }
                }
            }

            // setup natural potentials
            foreach (var _natural in Body.NaturalWeapons)
            {
                var _natPotential = new NaturalPotential(_natural);
                Budget.BudgetItems.Add(_natPotential.Source, _natPotential);
            }

            // attack potential factories yielding attack potentials (putting slotted ones last)
            foreach (var _potential in from _factory in _Budget.Actor.Adjuncts.OfType<IAttackPotentialFactory>()
                                       from _ap in _factory.GetIAttackPotentials(this)
                                       orderby (_ap is ISlotAttackPotential ? 2 : 1) ascending
                                       select _ap)
                Budget.BudgetItems.Add(_potential.Source, _potential);
        }

        public void Removed()
        {
            // remove off hand adjuncts
            var _main = _Creature.Adjuncts.OfType<OffHand>().Where(_mh => _mh.Source == this).ToList();
            foreach (var _mh in _main)
                _mh.Eject();

            _Budget.Actor.Actions.Providers.Remove(this);
            DoTerminate();
        }

        public string Name => @"Full Attack";
        public string Description => $@"Started: {AttackStarted}";

        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // setup potentials list
            var _potentials = _Budget.BudgetItems.Items.OfType<IAttackPotential>()
                    .OrderBy(_ap => _ap is ISlotAttackPotential ? 2 : 1)
                    .ToList();

            var _budget = budget as LocalActionBudget;
            if (AttackStarted)
            {
                // get attack strikes
                var _strikes = _budget.Actor.Actions.GetActionProviders()
                    .OfType<WeaponBase>()
                    .SelectMany(_w => _w.WeaponStrikes());
                if (_budget.CanPerformBrief)
                {
                    // capable of performing a full attack sequence
                    foreach (var _strike in _strikes)
                    {
                        if (_potentials.Any(_p => _p.CanUse(_strike)))
                        {
                            // setup sequencer and strike
                            yield return new FullAttackSequencer(_strike, _strike.OrderKey);
                        }
                    }
                }
                else if (_budget.TopActivity?.Action is FullAttackSequencer)
                {
                    // already sequencing
                    foreach (var _strike in _strikes)
                    {
                        if (_potentials.Any(_p => _p.CanUse(_strike)))
                        {
                            // strike while sequenced
                            yield return _strike;
                        }
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new Info { Message = Name };

        #endregion
    }
}
