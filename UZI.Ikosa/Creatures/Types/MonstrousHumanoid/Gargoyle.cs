using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Gargoyle : Species
    {
        // Gargoyle class skills are Hide, Listen, and Spot. 
        private readonly static Type[] _ClassSkills
            = new Type[]
            {
                typeof(StealthSkill),
                typeof(ListenSkill),
                typeof(SpotSkill)
            };

        // make sure the gargoyle's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;
        static Gargoyle()
        {
            _SizeRanges = new List<SizeRange>
            {
                new SizeRange(5, 6, Size.Medium, 1),
                new SizeRange(7, 12, Size.Large, 2)
            };
        }

        // NOTE: gargoyle's default abilities are for making a basic gargoyle
        public override AbilitySet DefaultAbilities()
            => new AbilitySet(10, 10, 10, 10, 11, 11);

        public override Species TemplateClone(Creature creature)
            => new Gargoyle();

        public override bool IsCharacterCapable
            => true;

        public override Type FavoredClass()
            => typeof(Fighter);

        protected override CreatureType GenerateCreatureType()
            => new MonstrousHumanoidType();

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new MonstrousHumanoidClass<Gargoyle>(_ClassSkills, 12, _SizeRanges, FractionalPowerDie, SmallestPowerDie, false);

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => (powerDieLevel == 4)
            ? MnemonicCode.Str
            : null;

        protected override int GenerateNaturalArmor()
            => 4;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new EarthSubType(this);
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 5,
                BaseLength = 5,
                BaseWeight = 200
            };

            return _body;
        }
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Wings", true, @"Wings");
            yield return new BodyFeature(this, @"Horns", true, @"Horns");
            yield return new BodyFeature(this, @"Claws", false, @"Claws");
            yield return new BodyFeature(this, @"Skin", false, @"Hard Rough Skin");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            // A gargoyle’s base land speed is 40 feet. It also has a fly speed of 60 feet (average).
            var _land = new LandMovement(40, Creature, this);
            yield return new FlightExMovement(60, Creature, this, FlightManeuverability.Average);
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

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new ToughnessFeat(powerDie, 1);

                case 3:
                    return new MultiAttackFeat(powerDie, 3);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _ClassSkills, new int[] { 1, 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A gargoyle has a +2 racial bonus on Hide, Listen, and Spot checks, 
            //                and an additional +8 bonus on Hide checks when it is concealed against a background of stone. 
            var _skill = new Delta(2, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
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
            // damage Reduction
            yield return DamageReductionTrait.GetDRMagicTrait(this, 10, _MonsterClass);

            // gargoyle needs item slots for secondary natural attacks
            yield return new ExtraordinaryTrait(this, @"Mouth Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                new ItemSlotTrait(this, ItemSlot.Mouth, string.Empty, false, false));
            yield return new ExtraordinaryTrait(this, @"Horns Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                new ItemSlotTrait(this, ItemSlot.Horns, string.Empty, false, false));

            // and needs all the natural attacks added (with a magical enhancement of 0)
            var _claw = new Claw(@"1d4", Size.Miniature, 20, 2, @"Main", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw));

            var _claw2 = new Claw(@"1d4", Size.Miniature, 20, 2, @"Off", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw2));

            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, false, false);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            var _gore = new Gore(@"1d6", Size.Tiny, 20, 2, false, false);
            yield return new ExtraordinaryTrait(this, @"Gore", @"Attack with horns", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _gore));

            yield return MagicNaturalWeaponsTrait.GetMagicNaturalWeaponsTrait(this, 0, _MonsterClass);

            // ability deltas: brings default or rolled abilites up to Gargoyle standards
            yield return new ExtraordinaryTrait(this, @"Gargoyle Strength", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Gargoyle Dexterity", @"+4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Gargoyle Constitution", @"+8", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(8, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Gargoyle Intelligence", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Gargoyle Charisma", @"-4", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-4, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // freeze
            var _statue = new Statue(StoneMaterial.Static);
            yield return new ExtraordinaryTrait(this, @"Freeze", @"Observers need spot check to notice creature is alive",
                TraitCategory.Quality,
                new FreezeTrait(this, new Deltable(20), ObjectInfoFactory.CreateInfo<ObjectInfo>(Creature, _statue)));
            yield break;
        }
        #endregion
    }
}
