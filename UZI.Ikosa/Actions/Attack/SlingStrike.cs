using System;
using System.Linq;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SlingStrike : RangedStrike
    {
        public SlingStrike(RangedAmmunition source, string orderKey)
            : base(source, Contracts.AttackImpact.Penetrating, orderKey)
        {
        }

        public SlingStrike(RangedAmmunition source, bool provokesMelee, string orderKey)
            : base(source, Contracts.AttackImpact.Penetrating, provokesMelee, orderKey)
        {
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (Weapon is Sling _sling)
            {
                // use the ammunition
                _sling.Ammunition = null;
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
                    {
                        _tCell = _line.BlockedCell.ToCellPosition();
                    }
                    else if (_line.UnblockedCell.IsActual)
                    {
                        _tCell = _line.UnblockedCell.ToCellPosition();
                    }
                    else
                    {
                        _tCell = _target.Attack.SourceCell.ToCellPosition();
                    }

                    // drop an instance of ammunition at target location
                    if (RangedAmmunition?.Ammunition is SlingAmmo _bullet)
                    {
                        // drop at target cell
                        var _dropData = new Drop(_critter, _critter.Setting as LocalMap, _tCell, false);
                        var _interact = new Interaction(_critter, this, _bullet, _dropData);
                        _bullet.HandleInteraction(_interact);
                    }
                }
            }
        }
    }
}
