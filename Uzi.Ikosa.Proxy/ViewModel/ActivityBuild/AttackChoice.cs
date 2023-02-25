using System.Collections.Generic;
using System.Windows;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AttackChoice : ViewModelBase
    {
        public AttackChoice(AttackTargeting targetBuilder)
        {
            _TargetBuilder = targetBuilder;
        }

        #region private data
        private AwarenessInfo _Awareness;
        private ICellLocation _TargetCell;
        private IGeometricRegion _Region;
        private AttackTargeting _TargetBuilder;
        #endregion

        // types of attack choices
        // - melee-style 0 or (1 max) := cell, awareness optional
        // - strike-style 0 or (1 max) := cell, awareness optional
        // - cell-ranged (blind attack; cell attack only) := cell
        // - ranged := awareness (cannot use on adjacent if target cell on adjacent is ticked)
        // - ranged with indirect := awareness, cell also

        public AwarenessInfo Awareness { get { return _Awareness; } set { _Awareness = value; } }
        public ICellLocation TargetCell { get { return _TargetCell; } set { _TargetCell = value; } }
        public IGeometricRegion TargetRegion { get { return _Region; } set { _Region = value; } }

        public AttackTargeting Targeting => _TargetBuilder;

        public Visibility CellPointerVisibility
            => TargetCell != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility GeometryPointerVisibility
            => TargetRegion != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AwarenessVisibility
            => (Awareness != null) && !_TargetBuilder.AimingMode.IsCellAimOnly
                    ? Visibility.Visible
                    : Visibility.Collapsed;
    }

    public class AttackChoiceComparer : IEqualityComparer<AttackChoice>
    {
        private CellLocationEquality _Compare = new CellLocationEquality();

        #region IEqualityComparer<AttackChoice> Members
        private bool AwarenessEquals(AttackChoice x, AttackChoice y)
        {
            return ((x.Awareness == null) && (y.Awareness == null))
                || ((x.Awareness != null) && (y.Awareness != null) && (y.Awareness.ID == x.Awareness.ID));
        }

        private bool TargetCellEquals(AttackChoice x, AttackChoice y)
        {
            return ((x.TargetCell == null) && (y.TargetCell == null))
                || ((x.TargetCell != null) && (y.TargetCell != null) && _Compare.Equals(y.TargetCell, x.TargetCell));
        }

        public bool Equals(AttackChoice x, AttackChoice y)
        {
            return AwarenessEquals(x, y) && TargetCellEquals(x, y);
        }

        public int GetHashCode(AttackChoice obj)
        {
            if (obj.TargetCell != null)
            {
                var _compare = new CellLocationEquality();
                return _compare.GetHashCode(obj.TargetCell);
            }
            else if (obj.Awareness != null)
            {
                return obj.Awareness.ID.GetHashCode();
            }
            return 0;
        }

        #endregion
    }
}
