using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InflictCriticalWounds : InflictLightWounds
    {
        public override string DisplayName => @"Inflict Critical Wounds";
        public override string Description => @"Inflicts 4d8 + 1 per caster level hit points.  (max + 20)";
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Inflict", DisplayName, new DiceRoller(4, 8), 20, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Inflict.Negative", _dice, @"Inflict Critical", EnergyType.Negative);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Inflict.Negative.Critical", _dice, @"Inflict Critical (Critical)", EnergyType.Negative);
            yield break;
        }
    }
}
