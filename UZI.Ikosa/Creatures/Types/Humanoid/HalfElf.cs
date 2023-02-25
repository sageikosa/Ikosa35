using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class HalfElf : BaseHumanoidSpecies
    {
        public HalfElf()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new HalfElf();

        public override Type FavoredClass() => null;
        public override string Name => @"Half-Elf";
        public override IEnumerable<Language> CommonLanguages() { yield break; }

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Elf>(this, @"Elf");
            yield return new CreatureSpeciesSubType<Human>(this, @"Human");
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2,
                BaseLength = 2,
                BaseWeight = 100
            };
            return _body;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, this.Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, this.Creature, this, false, _land);
            yield return new SwimMovement(0, this.Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // +1 racial bonus on Listen, Search, and Spot checks. An elf who merely passes within 5 feet of a secret or concealed door is entitled to a Search check to notice it as if she were actively looking for it. 
            var _skills1 = new Delta(1, this, @"Elf Racial Trait");
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skills1);
            yield return new KeyValuePair<Type, Delta>(typeof(SearchSkill), _skills1);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skills1);
            var _skills2 = new Delta(2, this, @"Elf Racial Trait");
            yield return new KeyValuePair<Type, Delta>(typeof(GatherInformationSkill), _skills1);
            yield return new KeyValuePair<Type, Delta>(typeof(DiplomacySkill), _skills1);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
            yield return new Elven(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Vision(true, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // +2 racial saving throw bonus against enchantment spells or effects
            yield return new ExtraordinaryTrait(this, @"Elven Save Bonuses", @"+2 saving throw bonus against enchantment spells or effects",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new ElfSaves(), Creature.FortitudeSave, Creature.WillSave, Creature.ReflexSave));

            // sleep immunity
            var _sleepImmune = new SpellImmunityHandler(true, typeof(Sleep));
            yield return new ExtraordinaryTrait(this, @"Elven Immunity", @"Immune to Sleep", TraitCategory.Quality,
                new InteractHandlerTrait(this, _sleepImmune));

            // breathes air
            yield return BreathesAir.GetTrait(this);

            yield break;
        }
        #endregion
    }
}
