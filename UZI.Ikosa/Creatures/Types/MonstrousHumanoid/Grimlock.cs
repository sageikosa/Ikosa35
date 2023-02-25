using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Grimlock : Species
    {
        // grimlock skills: climb, stealth, listen, spot
        private readonly static Type[] _ClassSkills
            = new Type[]
            {
                typeof(ListenSkill),
                typeof(ClimbSkill),
                typeof(StealthSkill),
                typeof(SpotSkill)
            };

        public override AbilitySet DefaultAbilities()
            => new AbilitySet(11, 11, 11, 10, 10, 10);

        public override Species TemplateClone(Creature creature)
            => new Grimlock();

        public override bool IsCharacterCapable
            => true;

        public override Type FavoredClass()
            => typeof(Barbarian);

        protected override CreatureType GenerateCreatureType()
            => new MonstrousHumanoidType();

        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            return string.Empty;
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield break;
        }

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new MonstrousHumanoidClass<Grimlock>(_ClassSkills, 2, FractionalPowerDie, SmallestPowerDie, false);

        protected override int GenerateNaturalArmor()
            => 4;

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2.5,
                BaseLength = 1,
                BaseWeight = 100
            };

            return _body;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            // A gargoyle’s base land speed is 40 feet. It also has a fly speed of 60 feet (average).
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

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
                    return new AlertnessFeat(powerDie, 1);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _ClassSkills, new int[] { 1, 1, 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
            yield return new Grimlockese(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Draconic(this);
            yield return new Dwarven(this);
            yield return new Languages.Gnome(this);
            yield return new Terran(this);
            yield return new Undercommon(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            // blindsight 40 feet. Vision and Hearing as well
            yield return new Senses.BlindSight(40, true, this);
            yield return new Senses.Hearing(this);
            yield return new Senses.Scent(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // ability deltas: brings default or rolled abilites up to Gargoyle standards
            yield return new ExtraordinaryTrait(this, @"Grimlock Strength", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Grimlock Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Grimlock Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Grimlock Wisdom", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Wisdom));
            yield return new ExtraordinaryTrait(this, @"Grimlock Charisma", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // proficiencies
            yield return new ExtraordinaryTrait(this, @"BattleAxe Proficiency", @"Proficient with the BattleAxe",
                TraitCategory.CombatHelper, new WeaponProficiencyTrait<BattleAxe>(this));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // +10 Stealth in mountains or underground
            // TODO: freeze: Difficulty=20 to notice Gargoyle is alive
            yield break;
        }
        #endregion
    }
}
