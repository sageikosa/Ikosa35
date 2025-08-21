using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InflictModerateWounds : InflictLightWounds
    {
        public override string DisplayName => @"Inflict Moderate Wounds";
        public override string Description => @"Inflicts 2d8 + 1 per caster level hit points.  (max + 10)";
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Inflict", DisplayName, new DiceRoller(2, 8), 10, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Inflict.Negative", _dice, @"Inflict Moderate", EnergyType.Negative);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Inflict.Negative.Critical", _dice, @"Inflict Moderate (Critical)", EnergyType.Negative);
            }

            yield break;
        }
    }
}
