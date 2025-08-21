using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class BlackBear : Species
    {
        #region size range and skills
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(ClimbSkill),
            typeof(ListenSkill),
            typeof(SpotSkill),
            typeof(SwimSkill)
        };

        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static BlackBear()
        {
            _SizeRanges =
            [
                new SizeRange(1, 5, Size.Medium, 1)
            ];
        }
        #endregion

        public override AbilitySet DefaultAbilities()
            => new AbilitySet(19, 13, 15, 2, 12, 6);

        /// <summary>Intelligence too low for character play</summary>
        public override bool IsCharacterCapable => false;

        // Standard: SupportsAbilityBoosts
        // Standard: FractionalPowerDie (1m)
        // Standard: Requirements (none)
        // Standard: Features (none)

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            if (powerDieLevel == 4)
            {
                return Abilities.MnemonicCode.Str;
            }

            return null;
        }
        #endregion

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    // TODO: endurance
                    return null;
                case 3:
                    // TODO: run
                    return null;
            }
            return null;
        }
        #endregion

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AnimalClass<BlackBear>(_Skills, 6, FractionalPowerDie, false);

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
            => new HumanoidBody(HideMaterial.Static, Size.Medium, 1, true);
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Shape", true, @"vaguely humanoid, but hunched almost like a dog");
            yield return new BodyFeature(this, @"Face", true, @"Snout-like with pointy ears");
            yield return new BodyFeature(this, @"Teeth", false, @"Sharp pointy teeth");
            yield return new BodyFeature(this, @"Fur", false, @"Dark black fur");
            yield break;
        }
        #endregion

        protected override CreatureType GenerateCreatureType()
            => new AnimalType();

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

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            return 2;
        }
        #endregion

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Vision(true, this);
            yield return new Senses.Hearing(this);
            yield return new Senses.Scent(this);
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NOTE: by 3rd PD, black-bear has +3 Listen and +3 Spot ...
            // ...Climb is pure ability and Swim is ability and bonus
            var _skills = new[] { typeof(ListenSkill), typeof(SpotSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1, 1 });
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // Racial Skills: A black bear has a +4 racial bonus on swim checks
            var _skill = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(SwimSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        protected override IEnumerable<Adjuncts.TraitBase> GenerateTraits()
        {
            // and needs all the natural attacks added
            var _claw = new Claw(@"1d4", Size.Tiny, 20, 2, @"Main", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw));

            var _claw2 = new Claw(@"1d4", Size.Tiny, 20, 2, @"Off", true, false);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw2));

            // need an item slot for the bite
            yield return new ExtraordinaryTrait(this, @"Mouth Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
               new ItemSlotTrait(this, ItemSlot.Mouth, string.Empty, false, false));

            // natural bite attack
            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, false, false);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: scent

            yield break;
        }

        public override Species TemplateClone(Creature creature)
            => new BlackBear();
    }
}
