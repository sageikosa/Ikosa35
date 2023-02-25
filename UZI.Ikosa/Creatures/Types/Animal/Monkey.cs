using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
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
    public class Monkey : Species
    {
        #region ctor
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(BalanceSkill),
            typeof(ClimbSkill),
            typeof(EscapeArtistSkill),
            typeof(StealthSkill),
            typeof(ListenSkill),
            typeof(SpotSkill)
        };

        private readonly static List<SizeRange> _SizeRanges;

        static Monkey()
        {
            _SizeRanges = new List<SizeRange>
            {
                new SizeRange(1, 1, Size.Tiny, 0),
                new SizeRange(2, 3, Size.Small, 1)
            };
        }
        #endregion

        public override AbilitySet DefaultAbilities()
            => new AbilitySet(3, 15, 10, 2, 12, 5);

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable => false;

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            return null;
        }
        #endregion

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new AgileFeat(powerDie, 1);
                case 3:
                    return new SkillFocusFeat<ClimbSkill>(powerDie, 3);
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

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<Monkey>(_Skills, 3, _SizeRanges, FractionalPowerDie, false);

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
            => new HumanoidBody(HideMaterial.Static, Size.Tiny, 0, false)
            {
                BaseHeight = 2,
                BaseWidth = 1,
                BaseLength = 1,
                BaseWeight = 15
            };
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Shape", true, @"Small wirey creature");
            yield return new BodyFeature(this, @"Face", true, @"Round-head");
            yield return new BodyFeature(this, @"Teeth", false, @"Human-like teeth");
            yield return new BodyFeature(this, @"Fur", false, @"Brown short fur");
            yield break;
        }
        #endregion

        protected override CreatureType GenerateCreatureType()
            => new AnimalType();

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(30, Creature, this, true, null);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override int GenerateNaturalArmor()
            => 0;

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
            yield return new Senses.Vision(true, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A monkey skills
            var _skill = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(BalanceSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // need an item slot for the bite
            yield return new ExtraordinaryTrait(this, @"Mouth Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
               new ItemSlotTrait(this, ItemSlot.Mouth, string.Empty, false, false));

            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Miniature, 20, 2, true, true);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // dexterity rules
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });
            var _better = new BetterDelta(Creature.Abilities.Ability<Dexterity>(), Creature.Abilities.Ability<Strength>());
            yield return new ExtraordinaryTrait(this, @"Nimble Climber", @"Uses Dexterity for Climb if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<ClimbSkill>()));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }

        public override Species TemplateClone(Creature creature)
            => new Monkey();
    }
}
