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
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Lizardfolk : BaseHumanoidSpecies
    {
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(BalanceSkill),
                typeof(JumpSkill),
                typeof(SwimSkill)
            };

        public Lizardfolk()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Lizardfolk();

        public override Type FavoredClass() => typeof(Cleric);  // TODO: Druid
        public override string Name => @"Lizardfold";
        public override bool IsCharacterCapable => true;

        // NOTE: lizardfolk's default abilities are for making a basic lizardfolk
        public override AbilitySet DefaultAbilities()
            => new AbilitySet(11, 10, 11, 11, 10, 10);

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Languages.Aquan(this);
            yield return new Languages.Goblin(this);
            yield return new Languages.Gnollish(this);
            yield return new Languages.Orcish(this);
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Draconic(this);
            yield return new Common(this);
            yield break;
        }
        #endregion

        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
            => new ReptilianSubStype(this).ToEnumerable();

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 6,
                BaseWidth = 3.5,
                BaseLength = 3.5,
                BaseWeight = 200
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
            var _skill = new Delta(4, this, @"Lizardfolk Racial Bonus");
            yield return new KeyValuePair<Type, Delta>(typeof(BalanceSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(JumpSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(SwimSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            // Vision and Hearing 
            yield return new Senses.Vision(false, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            yield return new ExtraordinaryTrait(this, @"Lizardfolk Strength", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Strength));
            yield return new ExtraordinaryTrait(this, @"Lizardfolk Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Lizardfolk Intelligence", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Intelligence));
            yield return new ExtraordinaryTrait(this, @"Lizardfolk Hold Breath", @"x4 time to hold breath",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new LizardFolkHoldBreathe(Creature),
                Creature.Abilities.Constitution));

            // proficiencies
            yield return new ExtraordinaryTrait(this, @"Simple Weapon Proficiency", @"Proficient with all simple weapons",
                TraitCategory.CombatHelper, new SimpleWeaponProficiencyTrait(this));
            yield return new ExtraordinaryTrait(this, @"Shield proficiency", @"Proficient with shields (except tower)",
                 TraitCategory.CombatHelper, new ShieldProficiencyTrait(this, false));

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

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
        #endregion

        [Serializable]
        public class LizardFolkHoldBreathe : IQualifyDelta
        {
            public LizardFolkHoldBreathe(Creature creature)
            {
                _Terminator = new TerminateController(this);
                _Critter = creature;
            }

            private Creature _Critter;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                if (qualify.Source is BreathlessCounter)
                {
                    // quadruple length without taking a breath, means + 3*CON
                    yield return new QualifyingDelta(_Critter.Abilities.Constitution.EffectiveValue * 3,
                        typeof(Racial), @"Lizard breath");
                }
                yield break;
            }

            #region ITerminating Members
            /// <summary>
            /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
            /// </summary>
            public void DoTerminate()
            {
                _Terminator.DoTerminate();
            }

            #region IControlTerminate Members
            private readonly TerminateController _Terminator;
            public void AddTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.AddTerminateDependent(subscriber);
            }

            public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.RemoveTerminateDependent(subscriber);
            }

            public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
            #endregion
            #endregion
        }

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new HumanoidClass<Lizardfolk>(_ClassSkills, 2, false, true, false, 0m, 0m, false);

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
                    return new MultiAttackFeat(powerDie, 1);
            }
            return null;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _distrib = new Type[]
            {
                typeof(BalanceSkill),
                typeof(JumpSkill),
                typeof(SwimSkill)
            };

            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _distrib,
                new int[] { 1, 2, 1 });
        }

        protected override int GenerateNaturalArmor() => 5;

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Claws", false, @"Claws");
            yield return new BodyFeature(this, @"Skin", false, @"Scaley skin");
            yield return new BodyFeature(this, @"Tail", false, @"Long thick tail");
            yield break;
        }
        #endregion
    }
}
