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
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Kobold : BaseHumanoidSpecies
    {
        public Kobold()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Kobold();

        public override Type FavoredClass()
            => typeof(Sorcerer);

        public override AbilitySet DefaultAbilities() { return new AbilitySet(13, 11, 12, 10, 9, 8); }

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Draconic(this);
            yield break;
        }
        #endregion

        #region public override IEnumerable<Languages.Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            // Common and Undercommon
            yield return new Common(this);
            yield return new Undercommon(this);
            yield break;
        }
        #endregion

        public override bool IsCharacterCapable
            => true;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Kobold>(this, @"Kobold");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Small, 1, false)
            {
                BaseHeight = 2,
                BaseWidth = 1.5,
                BaseLength = 1.5,
                BaseWeight = 40
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
            var _skill = new Delta(2, this, @"Kobold Racial Bonus");
            // craft trapmaking
            yield return new KeyValuePair<Type, Delta>(typeof(CraftSkill<CraftTrapMaking>), _skill);
            // profession miner
            // TODO: yield return new KeyValuePair<Type, Delta>(typeof(SearchSkill), _skill);
            // search
            yield return new KeyValuePair<Type, Delta>(typeof(SearchSkill), _skill);
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

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // +1 natural armor
            return 1;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // -4 STR, +2 DEX, -2 CON
            yield return new ExtraordinaryTrait(this, @"Kobold Strength", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Kobold Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Kobold Constitution", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Constitution));

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
