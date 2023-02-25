using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CureCriticalWounds : CureLightWounds
    {
        public override string DisplayName => @"Cure Critical Wounds";
        public override string Description => @"Cures 4d8 + 1 per caster level hit points.  (max + 20)";
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Cure", DisplayName, new DiceRoller(4, 8), 20, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Cure.Positive", _dice, @"Cure Critical", EnergyType.Positive);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Cure.Positive.Critical", _dice, @"Cure Critical (Critical)", EnergyType.Positive);
            yield break;
        }
    }
}
