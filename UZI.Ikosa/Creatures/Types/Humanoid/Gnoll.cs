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
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Gnoll : BaseHumanoidSpecies
    {
        // Gnoll class skills are Listen and Spot.
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(ListenSkill),
                typeof(SpotSkill)
            };

        public Gnoll() : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Gnoll();

        public override AbilitySet DefaultAbilities()
            => new AbilitySet(11, 10, 11, 10, 11, 10);

        public override Type FavoredClass()
            => typeof(Ranger); 

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Gnollish(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Languages.Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Common(this);
            yield return new Draconic(this);
            yield return new Elven(this);
            yield return new Languages.Goblin(this);
            yield return new Orcish(this);
            yield break;
        }
        #endregion

        public override bool IsCharacterCapable
            => true;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Gnoll>(this, @"Gnoll");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 7,
                BaseWidth = 4,
                BaseLength = 2,
                BaseWeight = 250
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
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            yield return new ExtraordinaryTrait(this, @"Gnoll Strength", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Gnoll Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Gnoll Intelligence", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Gnoll Charisma", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // proficiencies
            yield return new ExtraordinaryTrait(this, @"BattleAxe Proficiency", @"Proficient with the BattleAxe",
                TraitCategory.CombatHelper, new WeaponProficiencyTrait<BattleAxe>(this));
            yield return new ExtraordinaryTrait(this, @"Shortbow Proficiency", @"Proficient with the ShortBow",
                TraitCategory.CombatHelper, new WeaponProficiencyTrait<ShortBow>(this));
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
            => new HumanoidClass<Gnoll>(_ClassSkills, 2, true, false, false, 0m, 0m, false);

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            // Racial Hit Dice: A gnoll begins with two levels of humanoid, 
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
                    return new PowerAttackFeat(powerDie, 1);
            }
            return null;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _distrib = new Type[]
            {
                typeof(SpotSkill),
                typeof(ListenSkill)
            };

            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _distrib,
                new int[] { 1, 1 });
        }

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // +1 natural armor
            return 1;
        }
        #endregion

    }
}
