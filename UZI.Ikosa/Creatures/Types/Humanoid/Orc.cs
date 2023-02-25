using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Orc : BaseHumanoidSpecies
    {
        public Orc() : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Orc();

        public override AbilitySet DefaultAbilities() { return new AbilitySet(13, 11, 12, 10, 9, 8); }

        public override Type FavoredClass()
            => typeof(Barbarian);

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Languages.Orcish(this);
            yield return new Common(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Languages.Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            // Draconic, Elven, Giant, Gnoll, Orc
            yield return new Dwarven(this);
            yield return new Giant(this);
            yield return new Gnollish(this);
            yield return new Languages.Goblin(this);
            yield return new Undercommon(this);
            yield break;
        }
        #endregion

        public override bool IsCharacterCapable
            => true;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Orc>(this, @"Orc");
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
                BaseWeight = 140
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
            // STR +4, INT -2, WIS -2, CHA -2
            yield return new ExtraordinaryTrait(this, @"Orc Strength", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Orc Intelligence", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Orc Wisdom", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Wisdom));
            yield return new ExtraordinaryTrait(this, @"Orc Charisma", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Charisma));

            yield return new ExtraordinaryTrait(this, @"Simple Weapon Proficiency", @"Proficient with all simple weapons",
                TraitCategory.CombatHelper, new SimpleWeaponProficiencyTrait(this));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // light sensitivity
            yield return new ExtraordinaryTrait(this, @"Light Sensitivity", @"Dazzled in bright lights",
                TraitCategory.Quality, new LightSensitivity(this));

            yield break;
        }
        #endregion

    }
}
