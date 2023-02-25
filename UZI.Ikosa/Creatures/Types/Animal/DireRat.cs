using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class DireRat : Species, IDiseaseProvider
    {
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(ListenSkill),
            typeof(SpotSkill),
            typeof(SilentStealthSkill),
            typeof(StealthSkill),
            typeof(SwimSkill),
            typeof(ClimbSkill)
        };

        // make sure the Dire Rat's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static DireRat()
        {
            _SizeRanges = new List<SizeRange>
            {
                new SizeRange(1, 3, Size.Small, 1),
                new SizeRange(4, 6, Size.Medium, 1)
            };
        }

        public override AbilitySet DefaultAbilities() => new AbilitySet(10, 17, 12, 1, 12, 4);

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable => false;

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
                BaseWeight = 20
            };

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Face", true, @"Cone-shaped snout");
            yield return new BodyFeature(this, @"Teeth", true, @"Two sharp front teeth");
            yield return new BodyFeature(this, @"Tail", true, @"Long furless tail");
            yield return new BodyFeature(this, @"Legs", true, @"4 legs");
            yield return new BodyFeature(this, @"Fur", false, @"Oily fur");
            yield return new BodyFeature(this, @"Ears", false, @"Furless ears");
            yield return new BodyFeature(this, @"Eyes", false, @"Dark globes");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(40, Creature, this);
            yield return _land;
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield return new ClimbMovement(20, Creature, this, true, null);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<DireRat>(_Skills, 6, true, true, true, _SizeRanges, FractionalPowerDie, false);

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
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill), typeof(SilentStealthSkill), typeof(StealthSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1, 1, 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A dire rat has a +8 racial bonus on swim and climb 
            var _skill = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(SwimSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill);
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
            // disease...
            var _diseaseTrait = new DiseaseTrait(this, this);
            yield return new ExtraordinaryTrait(this, @"Disease", @"Filth Fever", TraitCategory.CombatHelper,
                _diseaseTrait);

            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, true, true);
            var _wpnDisease = new WeaponSecondarySpecialAttackResult(_diseaseTrait, false, true);
            _bite.MainHead.AddAdjunct(_wpnDisease);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // DEX for climb/swim
            var _better = new BetterDelta(Creature.Abilities.Ability<Dexterity>(), Creature.Abilities.Ability<Strength>());
            yield return new ExtraordinaryTrait(this, @"Nimble Climber", @"Uses Dexterity for Climb if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<ClimbSkill>()));
            yield return new ExtraordinaryTrait(this, @"Nimble Swimmer", @"Uses Dexterity for Swim if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<SwimSkill>()));

            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: scent?

            yield break;
        }

        #region IDiseaseProvider
        public Disease GetDisease()
        {
            var _con = new AbilityPoisonDamage(MnemonicCode.Con, new DieRoller(3));
            var _dex = new AbilityPoisonDamage(MnemonicCode.Dex, new DieRoller(3));
            return new Disease(@"Filth Fever", new PoisonMultiDamage(_con, _dex),
                new DieRoller(3), new Day(), 2,
                Creature.GetIntrinsicPowerDifficulty(Creature.Classes.Get<AnimalClass<DireRat>>(), MnemonicCode.Con, typeof(DiseaseTrait)),
                Disease.InfectionVector.Injury);
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new DireRat();
    }
}
