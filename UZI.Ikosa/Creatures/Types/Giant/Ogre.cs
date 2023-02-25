using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Ogre : Species
    {
        // class skills: Climb, Listen, and Spot.
        private readonly static Type[] _ClassSkills
            = new Type[]
            {
                typeof(ClimbSkill),
                typeof(ListenSkill),
                typeof(SpotSkill)
            };

        public override Species TemplateClone(Creature creature)
            => new Ogre();

        public override AbilitySet DefaultAbilities()
            => new AbilitySet(10, 10, 11, 10, 10, 11);

        public override bool IsCharacterCapable
            => true;

        protected override CreatureType GenerateCreatureType()
            => new GiantType();

        protected override int GenerateNaturalArmor()
            => 5;

        public override Type FavoredClass()
            => typeof(Barbarian);

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => (powerDieLevel == 4)
            ? MnemonicCode.Str
            : null;

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new GiantClass<Ogre>(_ClassSkills, 4, false);

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new ToughnessFeat(powerDie, 1);

                case 3:
                    return new WeaponFocusFeat<Greatclub>(powerDie, 3);
            }
            return null;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Giant(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Languages.Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            // Draconic, Elven, Giant, Gnoll, Orc
            yield return new Common(this);
            yield return new Dwarven(this);
            yield return new Orcish(this);
            yield return new Languages.Goblin(this);
            yield return new Terran(this);
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
            // Large size; Space/Reach: 10 feet/10 feet
            => new HumanoidBody(HideMaterial.Static, Size.Large, 2, false)
            {
                BaseHeight = 9,
                BaseWidth = 6,
                BaseLength = 3,
                BaseWeight = 600
            };

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Skin", true, @"Dull brown skin");
            yield return new BodyFeature(this, @"Coverings", true, @"Hides and furs");
            yield return new BodyFeature(this, @"Skin", false, @"Thick skin");
            yield return new BodyFeature(this, @"Odor", false, @"Stinky");
            yield break;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(40, Creature, this);
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
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // Racial Skills: skill points equal to 7 × (2 + Int modifier, minimum 1)
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _ClassSkills, new int[] { 0, 1, 1 });
        }

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // ability deltas: brings default or rolled abilites up to Ogre standards
            yield return new ExtraordinaryTrait(this, @"Ogre Strength", @"+10", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(10, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Ogre Dexterity", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Ogre Constitution", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Ogre Intelligence", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Ogre Charisma", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // proficiencies: simple, martial, light armor, medium armor and shields
            yield return new ExtraordinaryTrait(this, @"Simple Weapon Proficiency", @"Proficient with all simple weapons",
                TraitCategory.CombatHelper, new SimpleWeaponProficiencyTrait(this));
            yield return new ExtraordinaryTrait(this, @"Martial Weapon Proficiency", @"Proficient with all martial weapons",
                TraitCategory.CombatHelper, new MartialWeaponProficiencyTrait(this));
            yield return new ExtraordinaryTrait(this, @"Light Armor Proficiency", @"Proficient with light armor",
                TraitCategory.CombatHelper, new ArmorProficiencyTrait(this, Contracts.ArmorProficiencyType.Light));
            yield return new ExtraordinaryTrait(this, @"Medium Armor Proficiency", @"Proficient with medium armor",
                TraitCategory.CombatHelper, new ArmorProficiencyTrait(this, Contracts.ArmorProficiencyType.Medium));
            yield return new ExtraordinaryTrait(this, @"Shield proficiency", @"Proficient with shields (except tower)",
                 TraitCategory.CombatHelper, new ShieldProficiencyTrait(this, false));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
        #endregion
    }
}
