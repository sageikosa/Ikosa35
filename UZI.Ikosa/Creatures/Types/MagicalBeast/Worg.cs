using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Worg : Species
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

        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static Worg()
        {
            _SizeRanges =
            [
                new SizeRange(1, 6, Size.Medium, 1),
                new SizeRange(7, 12, Size.Large, 1)
            ];
        }
        #endregion

        public override AbilitySet DefaultAbilities() { return new AbilitySet(16, 15, 15, 6, 14, 10); }

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable { get { return false; } }

        // Standard: SupportsAbilityBoosts
        // Standard: FractionalPowerDie (1m)
        // Standard: Requirements (none)
        // Standard: Features (none)

        #region protected override CreatureType GenerateCreatureType()
        protected override CreatureType GenerateCreatureType()
        {
            return new MagicalBeastType();
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
                BaseHeight = 3,
                BaseWidth = 2,
                BaseLength = 5,
                BaseWeight = 300
            };
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Face", true, @"Snout");
            yield return new BodyFeature(this, @"Teeth", true, @"Sharp pointy teeth");
            yield return new BodyFeature(this, @"Legs", true, @"4 legs");
            yield return new BodyFeature(this, @"Fur", false, @"Dark gray fur");
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

        #region protected override BaseMonsterClass GenerateBaseMonsterClass()
        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            return new MagicalBeastClass<Worg>(_Skills, 12, FractionalPowerDie, SmallestPowerDie, false);
        }
        #endregion

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Repeat(PowerDieCalcMethod.Average, 4);

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new AlertnessFeat(powerDie, 1);
                case 3:
                    return new TrackFeat(powerDie, 1);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NOTE: by 4th PD (7 SP), worg has +1 Listen, +1 Spot and +3 Silent Stealth
            var _skills = new[] { typeof(SilentStealthSkill), typeof(ListenSkill), typeof(SpotSkill), typeof(LanguageSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 3, 1, 1, 2 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A worg has a +1 racial bonus on Listen, Silent Stealth, and Spot checks, 
            var _skill = new Delta(1, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(SilentStealthSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill);

            // Racial Skills: A worg has a +2 racial bonus on Hide checks, 
            var _skill2 = new Delta(2, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill2);
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
            yield return new Languages.Worg(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Darkvision(60, this);
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

        protected override IEnumerable<Adjuncts.TraitBase> GenerateTraits()
        {
            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, true, true);
            _bite.AddAdjunct(new NaturalTrip(this));
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: qualified delta to survival when tracking by scent

            // TODO: scent

            yield break;
        }

        public override Species TemplateClone(Creature creature)
            => new Worg();
    }
}
