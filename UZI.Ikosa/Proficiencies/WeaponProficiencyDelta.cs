using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa
{
    [Serializable]
    public class WeaponProficiencyDelta : ProficiencyDelta
    {
        public WeaponProficiencyDelta(ProficiencySet profSet)
            : base(profSet)
        {
        }

        /// <summary>Note this (currently) only accounts for opposed roll sources, not sunder/disarm targets</summary>
        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify is Interaction _iAct)
            {
                if ((_iAct.InteractData is AttackData)
                    || (_iAct.InteractData is SunderWieldedItemData)
                    || (_iAct.InteractData is DisarmData))
                {
                    if (qualify.Source is IWeaponHead _head)
                    {
                        var _weapon = _head.ContainingWeapon;
                        if (_weapon != null)
                        {
                            if (_weapon.IsProficiencySuitable(_iAct)
                                && _Set.IsProficientWith(_weapon, _Set.Creature.AdvancementLog.NumberPowerDice))
                            {
                                // no penalty
                                yield break;
                            }
                        }

                        // default non-proficiency (no valid weapon, or not in proficiency set
                        yield return new QualifyingDelta(-4, typeof(WeaponProficiencyDelta), @"Weapon non-proficiency");
                    }

                    // if attack not sourced by a weapon head, then we don't check proficiencies
                    // this is likely a spell source, grasp, probe etc...
                }
            }

            // default, no penalties (no valid interaction)
            yield break;
        }
    }
}