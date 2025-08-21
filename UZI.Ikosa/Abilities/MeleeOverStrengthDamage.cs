using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Abilities
{
    /// <summary>Adds 1.5 Strength Delta to damage if applicable</summary>
    [Serializable]
    public class MeleeOverStrengthDamage : MeleeStrengthDamage
    {
        /// <summary>Adds 1.5 Strength Delta to damage if applicable</summary>
        public MeleeOverStrengthDamage(Creature critter)
            : base(critter)
        {
        }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify.Source is IWeaponHead _wpnHead))
            {
                yield break;
            }

            // ranged attacks neved user over strength
            if (!(qualify is Interaction _iAct))
            {
                yield break;
            }

            if (_iAct.InteractData is RangedAttackData)
            {
                yield break;
            }

            if (_wpnHead.ContainingWeapon is NaturalWeapon)
            {
                var _natrl = _wpnHead.ContainingWeapon as NaturalWeapon;

                // secondary natural weapons get half STR damage
                if (!_natrl.IsPrimary)
                {
                    yield break;
                }

                // if more than one, and not treated as a sole natural weapon, then full STR damage
                if (Creature.Body.NaturalWeapons.Count > 1 && !_natrl.TreatAsSoleWeapon)
                {
                    yield break;
                }
            }
            else
            {
                // not a melee weapon
                if (!(_wpnHead.ContainingWeapon is IMeleeWeapon))
                {
                    yield break;
                }

                // off hand weapons get half STR damage
                if (_wpnHead.IsOffHand)
                {
                    yield break;
                }

                // must have two assigned slots behind the weapon head
                if (_wpnHead.AssignedSlots.OfType<HoldingSlot>().Count() < 2)
                {
                    yield break;
                }

                switch (_wpnHead.ContainingWeapon.GetWieldTemplate())
                {
                    case WieldTemplate.Unarmed:
                    case WieldTemplate.Light:
                        // light weapons never get 1.5 STR
                        yield break;
                }
            }

            // conditional STR boosts (lifting) won't affect DeltaValue
            var _delta = Creature.Abilities.Strength.DeltaValue;
            if (_delta > 0)
            {
                // 1.5 * bonus
                yield return new QualifyingDelta(_delta * 3 / 2, typeof(Strength), @"Strength");
            }
            else if (_delta < 0)
            {
                // full penalty
                yield return new QualifyingDelta(_delta, typeof(Strength), @"Strength");
            }
            yield break;
        }
    }
}
