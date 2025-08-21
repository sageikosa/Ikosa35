using System;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Handles basic movement budget (not distance, just action capacity) for a creature</summary>
    [Serializable]
    public class MovementBudget : IResetBudgetItem, ITurnEndBudgetItem
    {
        #region construction
        protected MovementBudget(Creature critter)
        {
            _Critter = critter;
            _HasMoved = false;
            _CanStillMove = true;
            _Heading = null;
        }
        #endregion

        #region data
        private Creature _Critter;
        private bool _HasMoved;
        private bool _CanStillMove;
        private int? _Heading = null;
        #endregion

        /// <summary>Indicates budget has already been used for movement, preventing some other actions</summary>
        public bool HasMoved { get { return _HasMoved; } set { _HasMoved = value; } }

        /// <summary>Indicates budget has not been used for a solitary move action already</summary>
        public bool CanStillMove { get { return _CanStillMove; } set { _CanStillMove = value; } }

        /// <summary>Heading of last movement step</summary>
        public int? Heading { get { return _Heading; } set { _Heading = value; } }

        #region IResetBudgetItem Members

        public bool Reset()
        {
            _HasMoved = false;
            _CanStillMove = true;
            foreach (var _move in _Critter.Movements.AllMovements)
            {
                _move.OnResetBudget();
            }

            return false;
        }

        #endregion

        public object Source => typeof(MovementBudget);

        #region public static MovementBudget GetBudget(CoreActionBudget budget)
        public static MovementBudget GetBudget(CoreActionBudget budget)
        {
            if (!(budget.BudgetItems[typeof(MovementBudget)] is MovementBudget _moveBudget))
            {
                _moveBudget = new MovementBudget(budget.Actor as Creature);
                budget.BudgetItems.Add(_moveBudget.Source, _moveBudget);
            }
            return _moveBudget;
        }
        #endregion

        #region IBudgetItem Members
        public void Added(CoreActionBudget budget) { }
        public void Removed() { }
        public string Name => @"Movement";

        public string Description
        {
            get
            {
                return string.Concat(HasMoved ? @"Moved / " : @"Not yet moved / ",
              CanStillMove ? @"Can still move" : @"Can no longer move");
            }
        }
        #endregion

        #region ITurnEndBudgetItem Members

        public bool EndTurn()
        {
            foreach (var _move in _Critter.Movements.AllMovements)
            {
                _move.OnEndTurn();
            }

            return false;
        }

        #endregion

        public MovementBudgetInfo ToMovementBudgetInfo()
        {
            var _info = this.ToBudgetItemInfo<MovementBudgetInfo>();
            _info.HasMoved = HasMoved;
            _info.CanStillMove = CanStillMove;
            _info.Heading = Heading;
            return _info;
        }
    }
}