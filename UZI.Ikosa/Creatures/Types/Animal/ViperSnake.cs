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

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class ViperSnake : Species, IPoisonProvider
    {
        #region size range and skills
        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _BiteRollers;

        private readonly static Type[] _Skills = new Type[]
        {
            typeof(BalanceSkill),
            typeof(ClimbSkill),
            typeof(StealthSkill),
            typeof(ListenSkill),
            typeof(SpotSkill),
            typeof(SwimSkill)
        };

        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static ViperSnake()
        {
            _SizeRanges =
            [
                new CustomSizeRange(0, 0, Size.Tiny, 0, 0,   0, 0, 0, 0),
                new CustomSizeRange(1, 1, Size.Small, 1, 1,  0, 2, 0, 1),
                new CustomSizeRange(2, 2, Size.Medium, 1, 1, 0, 2, 0, 0)
            ];

            // non-standard bite progression
            _BiteRollers = WeaponDamageRollers.BuildRollerProgression(
                @"-", @"-", @"1", @"1d2", @"1d4", @"1d4", @"1d6", @"1d6", @"1d8");
        }
        #endregion

        /// <summary>abilities for tiny viper</summary>
        public override AbilitySet DefaultAbilities()
            => new AbilitySet(4, 17, 11, 1, 12, 2);

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable => false;

        /// <summary>The smallest viper gets 1/3 PowerDie</summary>
        public override decimal FractionalPowerDie => 0.25m;

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            switch (powerDieLevel)
            {
                case 4:
                case 12:
                    return MnemonicCode.Str;

                case 8:
                case 16:
                    return MnemonicCode.Dex;
            }
            return null;
        }
        #endregion

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
            => powerDie.Level switch
            {
                // viper is too much of a mess to follow d20 SRD; 
                // following PFSRD instead and capping viper
                1 => new ImprovedInitiativeFeat(powerDie, 1),
                _ => null,
            };
        #endregion

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<ViperSnake>(_Skills, 2, FractionalPowerDie, false);

        protected override Body GenerateBody()
            => new SerpentBody(HideMaterial.Static, Size.Tiny, 0);

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Shape", true, @"narrow and coily");
            yield return new BodyFeature(this, @"Face", false, @"dark eyes, no ears, well spaced nostrils");
            yield return new BodyFeature(this, @"Teeth", true, @"pointy fangs");
            yield break;
        }
        #endregion

        protected override CreatureType GenerateCreatureType()
            => new AnimalType();

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(15, Creature, this);
            yield return new ClimbMovement(15, Creature, this, true, null);
            yield return new SwimMovement(15, Creature, this, false, null);
            yield break;
        }
        #endregion

        protected override int GenerateNaturalArmor()
            => 2;

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Vision(false, this);
            yield return new Senses.Hearing(this);
            yield return new Senses.Scent(this);
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill), typeof(SwimSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1, 1, 2 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A black bear has a +4 racial bonus on swim checks
            var _skill4 = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill4);
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skill4);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill4);
            var _skill8 = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(BalanceSkill), _skill8);
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill8);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            // TODO: snake sub-type...?
            yield break;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // poisonous
            var _poisonTrait = new PoisonTrait(this, new Poisonous(this));
            yield return new ExtraordinaryTrait(this, @"Poison", @"Poisonous Bite", TraitCategory.CombatHelper,
                _poisonTrait);

            // bite attack
            var _bite = new Bite(@"1", Size.Tiny, _BiteRollers, 20, 2, true, Contracts.DamageType.All, true);
            var _wpnPoison = new WeaponSecondarySpecialAttackResult(_poisonTrait, false, true)
            {
                PoweredUp = true
            };
            _bite.MainHead.AddAdjunct(_wpnPoison);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // better of STR/DEX for Climb
            var _better = new BetterDelta(Creature.Abilities.Ability<Dexterity>(), Creature.Abilities.Ability<Strength>());
            yield return new ExtraordinaryTrait(this, @"Nimble Climber", @"Uses Dexterity for Climb if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<ClimbSkill>()));

            // TODO: conditional +8 swim for special action or avoid hazard

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // weapon finesse (bonus feat, even when STR outstrips DEX at 6 power-dice)
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });

            // TODO: scent
            yield break;
        }

        #region public Poison GetPoison()
        public Poison GetPoison()
        {
            var _dmg = new AbilityPoisonDamage(MnemonicCode.Con, DieRoller.CreateRoller(StandardDieType.d6));

            // build poison
            var _class = Creature.Classes.Get<AnimalClass<ViperSnake>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Con, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Viper Poison", _dmg, _dmg,
                Poison.ActivationMethod.Injury,
                Poison.MaterialForm.Liquid,
                _difficulty);
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new ViperSnake();
    }
}
