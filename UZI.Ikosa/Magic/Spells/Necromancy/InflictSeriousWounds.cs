using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InflictSeriousWounds : InflictLightWounds
    {
        public override string DisplayName => @"Inflict Serious Wounds"; 
        public override string Description => @"Inflicts 3d8 + 1 per caster level hit points.  (max + 15)"; 
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Inflict", DisplayName, new DiceRoller(3, 8), 15, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Inflict.Negative", _dice, @"Inflict Serious", EnergyType.Negative);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Inflict.Negative.Critical", _dice, @"Inflict Serious (Critical)", EnergyType.Negative);
            }

            yield break;
        }
    }
}
