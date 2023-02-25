using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Dog : Species
    {
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(JumpSkill),
            typeof(ListenSkill),
            typeof(SpotSkill),
            typeof(SurvivalSkill)
        };

        public override AbilitySet DefaultAbilities() => new AbilitySet(13, 17, 15, 2, 12, 6);

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable => false;

        // Standard: SupportsAbilityBoosts
        // Standard: FractionalPowerDie (1m)
        // Standard: Requirements (none)
        // Standard: Features (none)

        protected override CreatureType GenerateCreatureType()
            => new AnimalType();

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
            => new QuadrupedBody(HideMaterial.Static, Size.Small, 1)
            {
                BaseHeight = 2,
                BaseWidth = 1.5,
                BaseLength = 2.5,
                BaseWeight = 30
            };

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Face", true, @"Snout");
            yield return new BodyFeature(this, @"Teeth", true, @"Sharp pointy teeth");
            yield return new BodyFeature(this, @"Legs", true, @"4 legs");
            yield return new BodyFeature(this, @"Fur", false, @"Fur");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(40, Creature, this);
            yield return _land;
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<Dog>(_Skills, 1, FractionalPowerDie, false);

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => PowerDieCalcMethod.Average.ToEnumerable();

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new AlertnessFeat(powerDie, 1);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A dog has a +4 racial bonus on jump checks, 
            var _skill = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(JumpSkill), _skill);
            yield break;
        }
        #endregion

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => null;

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Vision(true, this);
            yield return new Senses.Hearing(this);
            yield return new Senses.Scent(this);
            yield break;
        }
        #endregion

        protected override int GenerateNaturalArmor()
            => 1;

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, true, true);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // track bonus feat
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new TrackFeat(this, 1));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: qualified delta to survival when tracking by scent

            // TODO: scent

            yield break;
        }

        public override Species TemplateClone(Creature creature)
            => new Dog();
    }
}
