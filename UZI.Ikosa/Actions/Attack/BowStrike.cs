using System;
using System.Linq;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class BowStrike : RangedStrike
    {
        #region ctor()
        public BowStrike(RangedAmmunition source, IAmmunitionTypedBundle<Arrow, BowBase> container, string orderKey)
            : base(source, Contracts.AttackImpact.Penetrating, orderKey)
        {
            _Container = container;
        }

        public BowStrike(RangedAmmunition source, IAmmunitionTypedBundle<Arrow, BowBase> container, bool provokesMelee, string orderKey)
            : base(source, Contracts.AttackImpact.Penetrating, provokesMelee, orderKey)
        {
            _Container = container;
        }
        #endregion

        #region data
        private IAmmunitionTypedBundle<Arrow, BowBase> _Container;
        #endregion

        public IAmmunitionTypedBundle<Arrow, BowBase> Container => _Container;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // use the arrow from the container
            if (RangedAmmunition?.Ammunition is Arrow _arrow)
            {
                if (Container != null)
                {
                    Container.Use(_arrow);
                }
            }
            return base.OnPerformActivity(activity);
        }

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
            // need to figure out if we hit or missed
            var _atkFB = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _target = result.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            var _critter = result.AttackData.Attacker as Creature;
            if (!(_atkFB?.Hit ?? false) && (_target != null))
            {
                // chance of recovery = 50%
                if (DieRoller.RollDie(_critter.ID, 100, Key, DisplayName(_critter), _critter.ID) <= 50)
                {
                    var _planar = _critter.GetPlanarPresence();

                    // weapon will be somewhere along line (first blocked cell?)
                    // either to the target, or the finalized cell location...
                    var _tCell = _target.TargetCell;
                    var _line = GetSingleLine(_target.SourcePoint, _target.TargetPoint, _target.Target,
                        _target.Attack.SourceCell, _tCell, _planar);
                    if (_line.BlockedCell.IsActual)
                        _tCell = _line.BlockedCell.ToCellPosition();
                    else if (_line.UnblockedCell.IsActual)
                        _tCell = _line.UnblockedCell.ToCellPosition();
                    else
                        _tCell = _target.Attack.SourceCell.ToCellPosition();

                    // drop an instance of ammunition at target location
                    if (RangedAmmunition?.Ammunition is Arrow _arrow)
                    {
                        // drop at target cell
                        _arrow = _arrow.Clone() as Arrow;
                        var _dropData = new Drop(_critter, _critter.Setting as LocalMap, _tCell, false);
                        var _interact = new Interaction(_critter, this, _arrow, _dropData);
                        _arrow.HandleInteraction(_interact);
                    }
                }
            }
        }
    }
}
