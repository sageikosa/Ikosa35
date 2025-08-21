using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CureSeriousWounds : CureLightWounds
    {
        public override string DisplayName => @"Cure Serious Wounds"; 
        public override string Description => @"Cures 3d8 + 1 per caster level hit points.  (max + 15)"; 
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Cure", DisplayName, new DiceRoller(3, 8), 15, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Cure.Positive", _dice, @"Cure Serious", EnergyType.Positive);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Cure.Positive.Critical", _dice, @"Cure Serious (Critical)", EnergyType.Positive);
            }

            yield break;
        }
    }
}
