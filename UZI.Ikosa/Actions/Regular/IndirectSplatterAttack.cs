using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase (Regular)]</summary>
    [Serializable]
    public class IndirectSplatterAttack : DirectSplatterAttack
    {
        /// <summary>[ActionBase (Regular)]</summary>
        public IndirectSplatterAttack(ISplatterWeapon splatterer, string orderKey)
            : base(splatterer, orderKey)
        {
        }

        public override string Key { get { return @"Splatter.Indirect"; } }
        public override string DisplayName(CoreActor actor)
            => $@"Throw {_Splat.GetKnownName(actor)} to Splatter (Indirect)";

        /// <summary>
        /// Splatters in a 2x2x2 burst around the intended point.
        /// </summary>
        protected override CoreStep SplatterOnTarget(CoreActivity activity, IInteract interactor, Locator locator, ICellLocation targetCell)
        {
            // in case there was an interactor there
            if (interactor != null)
            {
                return base.SplatterOnTarget(activity, interactor, locator, targetCell);
            }

            // since we were attacking a point, simply perform that activity
            if (activity.Targets.Where(_t => _t.Key.Equals(@"Target")) is AttackTarget _target)
            {
                var _map = locator.Map;
                var _rAtk = _target.Attack as RangedAttackData;
                var _factory = new SegmentSetFactory(_map,
                    _rAtk.SourceCell?.ToCellPosition(), _rAtk.TargetCell?.ToCellPosition(),
                    ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect);
                var _newLine = _map.SegmentCells(_rAtk.AttackPoint, targetCell.Point3D(), _factory,
                    _rAtk.AttackLocator.PlanarPresence);
                if (_newLine.BlockedCell.IsActual)
                {
                    return new MultiNextStep(activity, SplatterAtEnd(activity, _newLine), null);
                }
                else
                {
                    // otherwise, if there is nothing solid at the target point when it gets there, drop it until it hits something
                    // ... but only let it drop so far, otherwise its simply gone...
                    var _dropLine = _map.SegmentCells(targetCell.Point3D(),
                        targetCell.Point3D() + _map.GetGravityDropVector3D(targetCell), _factory, 
                        _rAtk.AttackLocator.PlanarPresence);
                    if (_dropLine.BlockedCell.IsActual)
                    {
                        return new MultiNextStep(activity, SplatterAtEnd(activity, _dropLine), null);
                    }
                    else
                    {
                        _Splat.DoneUseItem();
                    }
                }
            }
            return null;
        }

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Target", @"Location to Attack", AttackImpact.Touch, _Splat.Lethality,
                true, 100, this, FixedRange.One, FixedRange.One, new FixedRange(_Splat.MaxRange));
            yield break;
        }
        #endregion
    }
}
