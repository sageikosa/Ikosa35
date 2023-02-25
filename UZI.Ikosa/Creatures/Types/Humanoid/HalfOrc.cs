using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class HalfOrc : BaseHumanoidSpecies
    {
        public HalfOrc()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new HalfOrc();

        public override Type FavoredClass() => typeof(Barbarian);
        public override string Name => @"Half-Orc";
        public override IEnumerable<Language> CommonLanguages() { yield break; }

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Orc>(this, @"Orc");
            yield return new CreatureSpeciesSubType<Human>(this, @"Human");
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2.5,
                BaseLength = 2.5,
                BaseWeight = 150
            };
            return _body;
        }

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

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
            yield return new Orcish(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            // Darkvision out to 60 feet. Vision and Hearing as well
            yield return new Vision(true, this);
            yield return new Darkvision(60, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Half-Orc Strength", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Half-Orc Intelligence", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Half-Orc Charisma", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Charisma));

            yield break;
        }
        #endregion
    }
}
