using System;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used by both Defensive Combat and Improved Defensive Combat</summary>
    [Serializable]
    public class DefensiveCombatBudget : IResetBudgetItem, IQualifyDelta, IMonitorChange<CoreActivity>
    {
        #region construction
        /// <summary>Used by both Defensive Combat and Improved Defensive Combat</summary>
        public DefensiveCombatBudget(object source, bool lockable, int attack, int armorRating, string name)
        {
            _Source = source;
            _Term = new TerminateController(this);
            _Atk = new Delta(attack, typeof(DefensiveCombatBudget), name);
            _AR = new QualifyingDelta(armorRating, typeof(DefensiveCombatBudget), name);
            _Locked = false;
            _Lockable = lockable;
            _Budget = null;
        }
        #endregion

        #region data
        private object _Source;
        private bool _Locked;   // an attack has been detected, cannot remove
        private bool _Lockable; // ... only if the budget is lockable
        private TerminateController _Term;
        private Delta _Atk;
        private IDelta _AR;
        private CoreActionBudget _Budget;
        #endregion

        public Delta Attack => _Atk;
        public IDelta ArmorRating => _AR;
        public bool IsLocked => _Locked;
        public bool IsLockable => _Lockable;

        #region IResetBudgetItem Members

        public bool Reset()
        {
            // remove budget item on reset
            return true;
        }

        #endregion

        public object Source => _Source; 

        #region IControlTerminate Members

        public void DoTerminate()
        {
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

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify is Interaction _iAct)
                && (_iAct?.InteractData is AttackData)
                && (_Budget?.Actor is Creature _critter)
                && _critter.CanDodge(_iAct))
            {
                yield return _AR;
            }
            yield break;
        }

        #endregion

        #region IBudgetItem Members

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
            if (budget.Actor is Creature _critter)
            {
                _critter.AddChangeMonitor(this);

                // combat
                _critter.RangedDeltable.Deltas.Add(_Atk);
                _critter.MeleeDeltable.Deltas.Add(_Atk);
                _critter.OpposedDeltable.Deltas.Add(_Atk);

                // armor rating
                _critter.NormalArmorRating.Deltas.Add(this);
                _critter.TouchArmorRating.Deltas.Add(this);
                _critter.IncorporealArmorRating.Deltas.Add(this);
            }
        }

        public void Removed()
        {
            if (_Budget.Actor is Creature _critter)
            {
                _critter.RemoveChangeMonitor(this);
            }

            // terminate delta and qualify delta
            _Atk.DoTerminate();
            DoTerminate();
        }

        public string Name
            => $@"Defensive Combat: {Source.SourceName()}";

        public string Description
            => $@"ATK:{Attack.Value}; Armor:{ArmorRating.Value}; Locked:{IsLocked}";

        #endregion

        #region IMonitorChange<CoreActivity> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args) { }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            if (args.Action.Equals(@"Start", StringComparison.OrdinalIgnoreCase))
            {
                if (_Budget == null)
                {
                    return;
                }

                // cannot drop the penalty if already attacked (unless the budget is not lockable)
                if (_Locked && _Lockable)
                {
                    return;
                }

                if (args.NewValue.Actor == _Budget.Actor)
                {
                    var _act = args.NewValue.Action as ActionBase;

                    if (_act != null)
                    {
                        // harmless choices/free actions do not affect
                        // NOTE: there are no free spells, they are at least Twitch
                        if (_act.IsHarmless && ((_act.TimeCost.ActionTimeType == TimeType.FreeOnTurn)
                                             || (_act.TimeCost.ActionTimeType == TimeType.Free)))
                        {
                            return;
                        }

                        // NOTE: Sub-Actions are only distinct when part of other actions
                        // NOTE: since the containing/controlling action didn't evict this budget, it must be OK!
                        if (_act.TimeCost.ActionTimeType == TimeType.SubAction)
                        {
                            return;
                        }
                    }

                    // lock on regular attack, finisher
                    if ((_act is RegularAttack) || (_act is FullAttackSequencer))
                    {
                        _Locked = true;
                    }
                    else
                    {
                        // remove defensive combat (did something that cancels its use)
                        _Budget.BudgetItems.Remove(Source);
                    }
                }
            }
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args) { }

        #endregion
    }
}
