using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public abstract class BaseHumanoidSpecies : Species
    {
        protected override CreatureType GenerateCreatureType()
            => new HumanoidType();

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield break;
        }

        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
            => null;

        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            yield break;
        }

        protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
            => null;

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            return;
        }

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => null;

        protected override int GenerateNaturalArmor()
            => 0;

        public override bool IsCharacterCapable
            => true;
    }
}
