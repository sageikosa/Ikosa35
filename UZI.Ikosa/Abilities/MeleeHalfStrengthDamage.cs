using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Abilities
{
    /// <summary>
    /// Qualified delta to apply half strength damage on a melee attack
    /// </summary>
    [Serializable]
    public class MeleeHalfStrengthDamage : MeleeStrengthDamage
    {
        public MeleeHalfStrengthDamage(Creature critter)
            : base(critter)
        {
        }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify.Source is IWeaponHead _wpnHead))
            {
                yield break;
            }

            if (_wpnHead.ContainingWeapon is NaturalWeapon)
            {
                var _natrl = _wpnHead.ContainingWeapon as NaturalWeapon;

                // primary natural weapons get full STR damage
                if (_natrl.IsPrimary)
                {
                    yield break;
                }

                // sole natural weapons get 1.5 STR damage
                if ((Creature.Body.NaturalWeapons.Count == 1) || _natrl.TreatAsSoleWeapon)
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

                // if assigned slots has any non off-hand, then no half-strength damage
                if (!_wpnHead.IsOffHand)
                {
                    yield break;
                }
            }

            // conditional STR boosts (lifting) won't affect DeltaValue
            var _delta = Creature.Abilities.Strength.DeltaValue;
            if (_delta > 0)
            {
                // half bonus
                yield return new QualifyingDelta(_delta / 2, typeof(Strength), @"Strength");
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
