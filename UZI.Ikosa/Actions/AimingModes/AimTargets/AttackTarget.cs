using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Contracts;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class AttackTarget : AimTarget
    {
        #region constructor
        public AttackTarget(string key, IInteract target, AttackData attack)
            : base(key, target)
        {
            _Attack = attack;
        }
        #endregion

        #region data
        private AttackData _Attack;
        #endregion

        public AttackData Attack => _Attack;

        public Point3D SourcePoint
            => Attack.SourceCell.GetPoint();

        /// <summary>
        /// explicit target cell centroid ?? middle of target locator ?? source point
        /// </summary>
        public Point3D TargetPoint
            => Attack?.TargetCell?.GetPoint()
            ?? (Target as ICoreObject)?.GetLocated()?.Locator.MiddlePoint
            ?? SourcePoint;

        /// <summary>
        /// explicit target cell ?? nearest target cell for target ?? source cell
        /// </summary>
        public ICellLocation TargetCell
        {
            get
            {
                var _sCell = new CellPosition(Attack.SourceCell);
                return Attack.TargetCell
                    ?? (from _c in (Target as ICoreObject)?.GetLocated()?.Locator.GeometricRegion.AllCellLocations()
                        let _cL = new CellPosition(_c)
                        select new
                        {
                            Cell = _cL,
                            Distance = _cL.NearDistanceToCell((ICellLocation)_sCell)
                        })
                        .OrderBy(_cd => _cd.Distance)
                        .Select(_cd => (ICellLocation)_cd.Cell)
                        .FirstOrDefault()
                    ?? Attack.SourceCell;
            }
        }

        public override AimTargetInfo GetTargetInfo()
        {
            var _info = ToInfo<AttackTargetInfo>();
            _info.IsNonLethal = Attack.IsNonLethal;
            _info.Impact = Attack.Impact;
            _info.InRange = Attack.InRange;
            _info.AttackScore = Attack.AttackScore?.EffectiveValue;
            _info.CriticalConfirm = Attack.CriticalConfirmation?.EffectiveValue;
            return _info;
        }
    }
}
