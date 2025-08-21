using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Wolf : Species
    {
        #region size range and skills
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(StealthSkill),
            typeof(ListenSkill),
            typeof(SilentStealthSkill),
            typeof(SpotSkill),
            typeof(SurvivalSkill)
        };

        // make sure the Wolf's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static Wolf()
        {
            _SizeRanges =
            [
                new SizeRange(1, 3, Size.Medium, 1),
                new SizeRange(4, 6, Size.Large, 1)
            ];
        }
        #endregion

        public override AbilitySet DefaultAbilities() { return new AbilitySet(13, 15, 15, 2, 12, 6); }

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable { get { return false; } }

        // Standard: SupportsAbilityBoosts
        // Standard: FractionalPowerDie (1m)
        // Standard: Requirements (none)
        // Standard: Features (none)

        #region protected override CreatureType GenerateCreatureType()
        protected override CreatureType GenerateCreatureType()
        {
            return new AnimalType();
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
            => new QuadrupedBody(HideMaterial.Static, Size.Medium, 1)
            {
                BaseHeight = 2.5,
                BaseWidth = 1.5,
                BaseLength = 4.5,
                BaseWeight = 95
            };
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Face", true, @"Snout");
            yield return new BodyFeature(this, @"Teeth", true, @"Sharp pointy teeth");
            yield return new BodyFeature(this, @"Legs", true, @"4 legs");
            yield return new BodyFeature(this, @"Fur", false, @"Dull gray fur");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(50, Creature, this);
            yield return _land;
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<Wolf>(_Skills, 6, _SizeRanges, FractionalPowerDie, false);

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new NaturalWeaponFocusFeat<Bite>(powerDie, 1);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NOTE: by 2nd PD, wolf has +2 Listen, +2 Spot and +1 Silent Stealth
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill), typeof(SilentStealthSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 2, 2, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            if (powerDieLevel == 4)
            {
                return Abilities.MnemonicCode.Str;
            }

            return null;
        }
        #endregion

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

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            return 2;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, true, true);
            _bite.AddAdjunct(new NaturalTrip(this));
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
            => new Wolf();
    }
}
