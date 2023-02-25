using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ShurikenStrike : RangedStrike
    {
        #region ctor()
        public ShurikenStrike(RangedAmmunition source, ShurikenGrip grip, string orderKey)
            : base(source, AttackImpact.Penetrating, orderKey)
        {
            _Grip = grip;
        }

        public ShurikenStrike(RangedAmmunition source, ShurikenGrip grip, bool provokesMelee, string orderKey)
            : base(source, AttackImpact.Penetrating, provokesMelee, orderKey)
        {
            _Grip = grip;
        }
        #endregion

        #region data
        private ShurikenGrip _Grip;
        #endregion

        public ShurikenGrip ShurikenGrip => _Grip;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // use the ammunition
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
                    if (RangedAmmunition?.Ammunition is Shuriken _shuriken)
                    {
                        // drop at target cell
                        var _dropData = new Drop(_critter, _critter.Setting as LocalMap, _tCell, false);
                        var _interact = new Interaction(_critter, this, _shuriken, _dropData);
                        _shuriken.HandleInteraction(_interact);
                    }
                }
            }

            // unslot weapon
            ShurikenGrip.Ammunition = null;
            ShurikenGrip.ClearSlots();
            ShurikenGrip.Possessor = null;
        }
    }
}
