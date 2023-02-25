using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RunLimiter : Adjunct, ITrackTime, IActionFilter
    {
        public RunLimiter()
            : base(typeof(RunLimiter))
        {
        }

        #region state
        private double _EndTime;
        private bool _ForceRecover;
        private decimal _RecoveryNeeded;
        private decimal _RunCounter;
        #endregion

        protected Creature Critter => Anchor as Creature;

        /// <summary>Creature's CON score</summary>
        public decimal RunMax
            => Critter.Abilities.Constitution.QualifiedValue(
                new Qualifier(Critter, typeof(RunLimiter), Critter));

        /// <summary>Count of rounds run since recovery was cleared</summary>
        public decimal RunCounter => _RunCounter;

        /// <summary>How much recovery needed when rest started</summary>
        public decimal RecoveryNeeded => _RecoveryNeeded;

        /// <summary>Ran to the max, cannot run until recovery cleared</summary>
        public bool IsForcedRecovery => _ForceRecover;

        /// <summary>Ran this round</summary>
        public bool IsStillRunning => (_RecoveryNeeded == _RunCounter);

        public void AddRound()
        {
            var _critter = Critter;

            // re-up end time since we just started another round of running
            // any running restarts the recovery time
            _EndTime = (_critter?.GetCurrentTime() ?? 0d) + Minute.UnitFactor;

            // increase the runRounds count, and lock in recover
            _RunCounter += 1m;
            _RecoveryNeeded = _RunCounter;

            // if run to the max, must rest
            if (_RunCounter >= RunMax)
            {
                _ForceRecover = true;
            }
        }

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            var _critter = Critter;
            _EndTime = (_critter?.GetCurrentTime() ?? 0d) + Minute.UnitFactor;

            // ran for 1 round, need to recover 1 round over a minute
            _RunCounter = 1m;
            _RecoveryNeeded = 1m;
            _ForceRecover = false;

            // action filter
            _critter.Actions.Filters.Add(this, this);

            // base
            base.OnActivate(source);
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            // base
            base.OnDeactivate(source);

            // action filter
            Critter?.Actions.Filters.Remove(this);
        }

        public override object Clone()
            => new RunLimiter();

        /// <summary>
        /// Suppress running if 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="budget"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if (IsForcedRecovery && (budget.Actor == Critter))
            {
                if (action is MovementAction _moveAct)
                {
                    switch (_moveAct.TimeNeeded.ActionTimeType)
                    {
                        case TimeType.Free:
                        case TimeType.Opportunistic:
                        case TimeType.Reactive:
                        case TimeType.FreeOnTurn:
                        case TimeType.Twitch:
                        case TimeType.SubAction:
                            // allow the action, but always force single...
                            MovementRangeBudget.GetBudget(budget)?.ForceSingle();
                            break;

                        case TimeType.Brief:
                            // always force single...
                            MovementRangeBudget.GetBudget(budget)?.ForceSingle();

                            // suppress if another brief move action and already moved
                            var _moveBudget = MovementBudget.GetBudget(budget);
                            return _moveBudget?.HasMoved ?? false;

                        case TimeType.Regular:
                        case TimeType.Total:
                        case TimeType.Span:
                        case TimeType.TimelineScheduling:
                        default:
                            // too much for a move action while limited
                            return true;
                    }
                }
            }
            return false;
        }

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Leaving)
            {
                // must have stopped to consider ejecting or decreasing
                if (!IsStillRunning)
                {
                    if ((timeVal >= _EndTime) || (_RunCounter <= 0m))
                    {
                        // done limiting 
                        Eject();
                    }
                    else
                    {
                        // takes a minute (10 rounds) to fully recover from running
                        _RunCounter -= RecoveryNeeded / 10m;
                    }
                }
            }
        }

        public double Resolution => Round.UnitFactor;
    }
}
