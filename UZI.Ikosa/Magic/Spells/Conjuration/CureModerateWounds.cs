using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CureModerateWounds : CureLightWounds
    {
        public override string DisplayName => @"Cure Moderate Wounds";
        public override string Description => @"Cures 2d8 + 1 per caster level hit points.  (max + 10)";
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Cure", DisplayName, new DiceRoller(2, 8), 10, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Cure.Positive", _dice, @"Cure Moderate", EnergyType.Positive);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Cure.Positive.Critical", _dice, @"Cure Moderate (Critical)", EnergyType.Positive);
            yield break;
        }
    }
}
