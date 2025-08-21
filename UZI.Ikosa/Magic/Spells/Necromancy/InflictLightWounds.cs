using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InflictLightWounds : InflictMinorWounds
    {
        public override string DisplayName => @"Inflict Light Wounds";
        public override string Description => @"Inflicts 1d8 + 1 per caster level hit points.  (max + 5)";
        public override IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Inflict", DisplayName, new DieRoller(8), 5, new ConstantRoller(1), 1);
            yield return new EnergyDamageRule(@"Inflict.Negative", _dice, @"Inflict Light", EnergyType.Negative);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Inflict.Negative.Critical", _dice, @"Inflict Light (Critical)", EnergyType.Negative);
            }

            yield break;
        }

        #region ISpellSaveMode Members
        public override SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            var _critter = workSet.Target as Creature;
            if (!(_critter.CreatureType is UndeadType))
            {
                return new SaveMode(SaveType.Will, SaveEffect.Half, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
            }
            return null;
        }
        #endregion
    }
}
