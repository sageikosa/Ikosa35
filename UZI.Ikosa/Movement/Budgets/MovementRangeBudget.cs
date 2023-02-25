using System;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using System.Diagnostics;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Movement
{
    /// <summary>
    /// Handles step by step budget tracking
    /// </summary>
    [Serializable]
    public class MovementRangeBudget : IResetBudgetItem
    {
        #region protected MovementRangeBudget()
        protected MovementRangeBudget()
        {
            _Remaining = 1d;
            _Double = 1d;
            _DiagFlag = false;
            _Stealth = Stealth.None;
            _Half = null;
            _StealthRoll = 10;
        }
        #endregion

        #region state
        private double _Remaining;
        private double _Double;
        private bool _DiagFlag;

        // stealth tracking
        private Stealth _Stealth;
        private Delta _Half;
        private int _StealthRoll;
        private double? _StealthExpire;
        #endregion

        public enum Stealth { None, Hasty, High };

        public object Source => typeof(MovementRangeBudget);

        /// <summary>Remaining movment budget range</summary>
        public double Remaining => _Remaining;

        /// <summary>Remaining movement budget for double move</summary>
        public double Double => _Double;

        private double HalfAdjustment(MovementBase movement)
            => Math.Pow(2d, Math.Max((movement?.CoreObject as Creature)?.MoveHalfing.EffectiveValue ?? 0d, 0d));

        private double Needed(MovementBase movement, bool diagonal, double cost)
            => (((diagonal && _DiagFlag) ? 2 * cost : cost) * 5) / (movement.EffectiveValue / HalfAdjustment(movement));

        /// <summary>True if the total budget can accomodate the additional distance.  If high-stealth, then double not counted.</summary>
        public bool CanMove(MovementBase movement, bool diagonal, double cost)
            => Needed(movement, diagonal, cost) <= (_Remaining + (_Stealth != Stealth.High ? _Double : 0));

        /// <summary>True if the movement will requires dipping into double move action budget</summary>
        public bool RequiresDouble(MovementBase movement, bool diagonal, double cost)
            => Needed(movement, diagonal, cost) > _Remaining;

        #region public static MovementRangeBudget GetBudget(CoreActionBudget budget)
        /// <summary>
        /// Gets the currently defined MovementRangeBudget.  
        /// If none are defined, a new one is created and added to the CoreActionBudget
        /// </summary>
        public static MovementRangeBudget GetBudget(CoreActionBudget budget)
        {
            if (!(budget?.BudgetItems[typeof(MovementRangeBudget)] is MovementRangeBudget _moveBudget))
            {
                _moveBudget = new MovementRangeBudget();
                budget.BudgetItems.Add(_moveBudget.Source, _moveBudget);
            }
            return _moveBudget;
        }
        #endregion

        #region public void ForceDouble()
        /// <summary>Forces the movment to use total effort</summary>
        public void ForceDouble()
        {
            _Remaining += _Double;
            _Double = 0;
        }
        #endregion

        /// <summary>Prevents any use of double movement (such as when only partial effort can be used)</summary>
        public void ForceSingle() { _Double = 0; }

        public Stealth CurrentStealth => _Stealth;
        public int StealthRoll => _StealthRoll;
        public double? StealthCheckExpire => _StealthExpire;

        public void SetStealthCheck(double expireTime, int rollValue)
        {
            _StealthExpire = expireTime;
            _StealthRoll = rollValue;
        }

        #region private void EnsureStealthHalf(Creature critter)
        private void EnsureStealthHalf(Creature critter)
        {
            var _halfing = critter?.MoveHalfing;
            if (_halfing != null)
            {
                if (!_halfing.Deltas.Any(_d => _d.Source == this))
                {
                    _Half = new Delta(1, this, @"Stealth");
                    _halfing.Deltas.Add(_Half);
                }
            }
        }
        #endregion

        #region public void SetStealth(Creature critter, Stealth stealth)
        public void SetStealth(Creature critter, Stealth stealth)
        {
            switch (stealth)
            {
                case Stealth.High:
                    // must still be in first part of movement
                    if (Double > 0)
                    {
                        // set stealth
                        _Stealth = stealth;
                        EnsureStealthHalf(critter);

                        // already started moving
                        if (Remaining < 1)
                        {
                            // ergo, must force a single move
                            ForceSingle();
                        }
                    }
                    break;

                case Stealth.Hasty:
                    _Stealth = stealth;
                    EnsureStealthHalf(critter);
                    break;

                case Stealth.None:
                default:
                    _Stealth = stealth;
                    if (_Half != null)
                    {
                        _Half.DoTerminate();
                        _Half = null;
                    }
                    break;
            }
        }
        #endregion

        #region public void DoMove(MovementAction moveAct, bool diagonal, int cost)
        /// <summary>Register the movement square</summary>
        public void DoMove(MovementAction moveAct, bool diagonal, double cost)
        {
            if (_Stealth == Stealth.High)
            {
                // NOTE: once movement is performed with high stealth, then only a single move can be performed
                ForceSingle();
            }

            var _needed = Needed(moveAct.Movement, diagonal, cost);

            // remaining larger than 1 to start; occurs if ForceDouble was called for a total move
            var _reserves = ((_Remaining > 1) && ((_Remaining - _needed) <= 1));

            _Remaining -= _needed;
            if (_Remaining < 0)
            {
                // move double into remaining (assume double was checked and confirmed in order to get here)
                ForceDouble();
                moveAct.Movement.OnSecondIncrementOfTotal(moveAct);
            }

            // flip diagonal flag
            if (diagonal)
                _DiagFlag = !_DiagFlag;

            // tapped into reserves, let the movement know it
            if (_reserves)
                moveAct.Movement.OnSecondIncrementOfTotal(moveAct);
        }
        #endregion

        #region IResetBudgetItem Members
        public bool Reset()
        {
            _Remaining = _Double = 1d;
            _DiagFlag = false;
            return false;
        }
        #endregion

        #region IBudgetItem Members
        public void Added(CoreActionBudget budget) { }
        public void Removed() { }
        public string Name => @"Movement Range";
        public string Description => $@"{Remaining:P} remaining ({Double:P} double)";
        #endregion

        #region public MovementRangeBudgetInfo ToMovementRangeBudgetInfo(CoreActionBudget coreBudget)
        public MovementRangeBudgetInfo ToMovementRangeBudgetInfo(CoreActionBudget coreBudget)
        {
            var _info = this.ToBudgetItemInfo<MovementRangeBudgetInfo>();

            var _budget = coreBudget as LocalActionBudget;
            if (!(_budget.TopActivity?.Action is MovementAction))
            {
                // not moving at the moment
                _info.Remaining = _budget.CanPerformBrief ? Remaining : 0d;
                _info.Double = _budget.CanPerformTotal ? Double : 0d;
            }
            else
            {
                // currently moving
                _info.Remaining = Remaining;
                _info.Double = _budget.CanPerformBrief ? Double : 0d;
            }

            if (_budget.BudgetItems[typeof(MovementBudget)] is MovementBudget _moveBudget)
            {
                if (!_moveBudget.CanStillMove)
                {
                    // no movement possible
                    _info.Remaining = 0d;
                    _info.Double = 0d;
                }
            }

            return _info;
        }
        #endregion
    }
}
