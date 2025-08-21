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
    public class Owl : Species
    {
        #region size range and skills
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(ListenSkill),
            typeof(SilentStealthSkill),
            typeof(SpotSkill)
        };

        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static Owl()
        {
            _SizeRanges =
            [
                new SizeRange(1, 1, Size.Tiny, 0),
                new SizeRange(2, 2, Size.Small, 1)
            ];
        }
        #endregion

        public override AbilitySet DefaultAbilities() => new AbilitySet(4, 17, 10, 2, 14, 4);

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
            => new AvianBody(HideMaterial.Static, Size.Tiny, 0, false)
            {
                BaseHeight = 0.5,
                BaseWidth = 3.5,
                BaseLength = 2,
                BaseWeight = 3
            };


        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Face", true, @"Large round face");
            yield return new BodyFeature(this, @"Eyes", true, @"Large round eyes");
            yield return new BodyFeature(this, @"Beak", true, @"Hooked beak");
            yield return new BodyFeature(this, @"Wings", true, @"Wide wings");
            yield return new BodyFeature(this, @"Talons", true, @"2 sharp talons");
            yield return new BodyFeature(this, @"Feathers", false, @"Feathers");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new FlightExMovement(40, Creature, this, FlightManeuverability.Average);
            var _land = new LandMovement(10, Creature, this);
            yield return _land;
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<Owl>(_Skills, 2, FractionalPowerDie, false);

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
            // Racial Skills: An owl has a +8 racial bonus on listen checks, +14 on silent-stealth
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), new Delta(8, typeof(Racial)));
            yield return new KeyValuePair<Type, Delta>(typeof(SilentStealthSkill), new Delta(14, typeof(Racial)));
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
            yield break;
        }
        #endregion

        protected override int GenerateNaturalArmor()
            => 2;

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // natural talons attack
            var _talon = new Claw(@"1d8", Size.Tiny, 20, 2, string.Empty, true, true);
            yield return new ExtraordinaryTrait(this, @"Talons", @"Attack with talons", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _talon));

            // weapon finesse bonus feat
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });

            // TODO: spot checks in shadowy

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            yield break;
        }

        public override Species TemplateClone(Creature creature)
            => new Owl();
    }
}
