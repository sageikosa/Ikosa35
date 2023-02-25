using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Troglodyte : BaseHumanoidSpecies, IPoisonProvider, IStenchGeometryBuilderFactory
    {
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(StealthSkill),
                typeof(ListenSkill)
            };

        public Troglodyte()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Troglodyte();

        public override Type FavoredClass() => typeof(Cleric);
        public override string Name => @"Troglodyte";
        public override bool IsCharacterCapable => true;

        // NOTE: Troglodyte's default abilities are for making a basic Troglodyte
        public override AbilitySet DefaultAbilities()
            => new AbilitySet(10, 11, 10, 10, 10, 10);

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Languages.Common(this);
            yield return new Languages.Giant(this);
            yield return new Languages.Goblin(this);
            yield return new Languages.Orcish(this);
        }
        #endregion

        protected override IEnumerable<Language> GenerateAutomaticLanguages()
            => new Draconic(this).ToEnumerable();

        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
            => new ReptilianSubStype(this).ToEnumerable();

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 3,
                BaseLength = 3,
                BaseWeight = 150
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
            var _skill = new Delta(4, this, @"Troglodyte Racial Bonus");
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill);   // TODO: +8 in rocky underground
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            // Darkvision, Vision and Hearing 
            yield return new Senses.Vision(false, this);
            yield return new Senses.Darkvision(90, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new HumanoidClass<Troglodyte>(_ClassSkills, 2, true, false, false, 0m, 0m, false);

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            // Racial Hit Dice: A bugbear begins with two levels of humanoid, 
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
                    return new WeaponFocusFeat<Javelin>(powerDie, 1);
            }
            return null;
        }
        #endregion

        protected override int GenerateNaturalArmor() => 6;

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Claws", false, @"Claws");
            yield return new BodyFeature(this, @"Skin", false, @"Diffuse scaley skin");
            yield return new BodyFeature(this, @"Tail", false, @"Tail");
            yield break;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _distrib = new Type[]
            {
                typeof(ListenSkill),
                typeof(StealthSkill)
            };

            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _distrib,
                new int[] { 1, 1 });
        }

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // abilities
            yield return new ExtraordinaryTrait(this, @"Troglodyte Dexterity", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Troglodyte Constitution", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Troglodyte Intelligence", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Intelligence));

            // bonus feat
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality, new MultiAttackFeat(this, 1));

            // natural weapons...
            yield return new ExtraordinaryTrait(this, @"Mouth Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                new ItemSlotTrait(this, ItemSlot.Mouth, string.Empty, false, false));
            var _claw = new Claw(@"1d4", Size.Miniature, 20, 2, @"Main", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw));

            var _claw2 = new Claw(@"1d4", Size.Miniature, 20, 2, @"Off", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw2));

            var _bite = new Bite(@"1d4", Size.Tiny, 20, 2, false, false);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // poison
            yield return new ExtraordinaryTrait(this, @"Troglodyte stench", @"Sicken nearby creatures", TraitCategory.Quality,
                new AdjunctTrait(this, new CreatureStenchControl(
                    new SpeciesStench<Troglodyte>(this, this) { InitialActive = false }, 5, true)));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }

        public Poison GetPoison()
        {
            var _sickened = new SickenedPoisonDamage(new ConstantRoller(10), new Round());
            var _none = new NoPoisonDamage();

            // build poison
            var _class = Creature.Classes.Get<HumanoidClass<Troglodyte>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Con, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Troglodyte Musk", _sickened, _none,
                Poison.ActivationMethod.Inhaled, Poison.MaterialForm.Liquid, _difficulty,
                Creature.ID, Hour.UnitFactor * 24);
        }

        public IGeometryBuilder GetStenchGeometryBuilder()
        {
            // create new one
            var _loc = Locator.FindFirstLocator(Creature);
            if (_loc != null)
            {
                var _expectEven = _loc.LocationAimMode == LocationAimMode.Intersection;

                // define new capture
                var _critterSize = (_loc.Chief == Creature
                    ? Creature.Body.Sizer.Size.CubeSize()
                    : _loc.NormalSize as IGeometricSize);
                var _stenchBump = (int)(new[] { _critterSize.ZHeight, _critterSize.YLength, _critterSize.XLength }).Max();
                var _reachZone = new SphereBuilder(12 + _stenchBump);
                // TODO: should have a spread instead of a cubic for the capture zone

                // cubic capture
                return new SphereBuilder(12 + _stenchBump);
            }
            return new SphereBuilder(12);
        }
    }
}
