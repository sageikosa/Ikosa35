using System;
using System.Collections.Generic;
using System.Linq;
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
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Bugbear : BaseHumanoidSpecies
    {
        // Bugbear class skills are Climb, Stealth, Listen, SilentStealth, Search and Spot. 
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(ClimbSkill),
                typeof(StealthSkill),
                typeof(ListenSkill),
                typeof(SilentStealthSkill),
                typeof(SearchSkill),
                typeof(SpotSkill)
            };

        public Bugbear() : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Bugbear();

        public override AbilitySet DefaultAbilities() { return new AbilitySet(11, 10, 11, 10, 10, 11); }

        public override Type FavoredClass()
            => typeof(Rogue);

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Languages.Goblin(this);
            yield return new Common(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Languages.Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            // Draconic, Elven, Giant, Gnoll, Orc
            yield return new Draconic(this);
            yield return new Elven(this);
            yield return new Gnollish(this);
            yield return new Giant(this);
            yield return new Orcish(this);
            yield break;
        }
        #endregion

        public override bool IsCharacterCapable
            => true;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Goblin>(this, @"Goblinoid");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 3,
                BaseLength = 3,
                BaseWeight = 120
            };
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            var _skill = new Delta(4, this, @"Goblin Racial Bonus");
            yield return new KeyValuePair<Type, Delta>(typeof(SilentStealthSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            //Darkvision out to 60 feet. Vision and Hearing as well
            yield return new Senses.Vision(false, this);
            yield return new Senses.Darkvision(60, this);
            yield return new Senses.Hearing(this);
            // TODO: scent
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            yield return new ExtraordinaryTrait(this, @"Bugbear Strength", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Bugbear Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Bugbear Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Bugbear Charisma", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // proficiencies
            yield return new ExtraordinaryTrait(this, @"Simple Weapon Proficiency", @"Proficient with all simple weapons",
                TraitCategory.CombatHelper, new SimpleWeaponProficiencyTrait(this));
            yield return new ExtraordinaryTrait(this, @"Light Armor Proficiency", @"Proficient with light armor",
                TraitCategory.CombatHelper, new ArmorProficiencyTrait(this, Contracts.ArmorProficiencyType.Light));
            yield return new ExtraordinaryTrait(this, @"Shield proficiency", @"Proficient with shields (except tower)",
                 TraitCategory.CombatHelper, new ShieldProficiencyTrait(this, false));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            return new HumanoidClass<Bugbear>(_ClassSkills, 3, false, true, false, 0m, 0m, false);
        }

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            // Racial Hit Dice: A bugbear begins with three levels of humanoid, 
            yield return PowerDieCalcMethod.Average;
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

                case 3:
                    return new WeaponFocusFeat<MorningStar>(powerDie, 3);
            }
            return null;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _distrib = new Type[]
            {
                typeof(StealthSkill),
                typeof(SilentStealthSkill),
                typeof(SpotSkill),
                typeof(ListenSkill),
                typeof(ClimbSkill)
            };

            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _distrib,
                new int[] { 3, 3, 1, 1, 1 });
        }

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // +3 natural armor
            return 3;
        }
        #endregion

    }
}
