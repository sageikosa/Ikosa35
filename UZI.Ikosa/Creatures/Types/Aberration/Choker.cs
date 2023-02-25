using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
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
    public class Choker : Species
    {
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(ClimbSkill),
            typeof(StealthSkill),
            typeof(SilentStealthSkill)
        };

        public override Species TemplateClone(Creature creature)
            => new Choker();

        public override bool IsCharacterCapable => false;

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => powerDieLevel switch
            {
                4 => MnemonicCode.Dex,
                8 => MnemonicCode.Dex,
                12 => MnemonicCode.Con,
                _ => string.Empty
            };

        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
            => powerDie.Level switch
            {
                1 => new LightningReflexesFeat(this, 1),
                3 => new StealthyFeat(this, 3),
                _ => null
            };

        protected override IEnumerable<Language> GenerateAutomaticLanguages()
            => new Undercommon(this).ToEnumerable();

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new AberrationClass<Choker>(_Skills, 12, FractionalPowerDie, SmallestPowerDie, false);

        protected override CreatureType GenerateCreatureType()
            => new AberrationType();

        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
            => Enumerable.Empty<CreatureSubType>();

        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(20, Creature, this);
            yield return _land;
            yield return new SwimMovement(0, Creature, this, false, _land);

            // natural climber
            yield return new ClimbMovement(10, Creature, this, false, null);
            yield break;
        }

        protected override int GenerateNaturalArmor()
            => 4;

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Repeat(PowerDieCalcMethod.Average, 3);

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Darkvision(60, this);
            yield return new Senses.Vision(true, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            var _skill = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill);
            yield break;
        }

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // climb            13  =8Rc +3Ab +0   +2SP
            // stealth          10  =4Sz +2Ab +2Ft +2SP
            // silent stealth   6   =0   +2Ab +2Ft +2SP
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _Skills, new int[] { 1, 1, 1 });
        }

        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Small, 2, false)
            {
                BaseHeight = 3.25,
                BaseWidth = 1.5,
                BaseLength = 1.5,
                BaseWeight = 35
            };
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Arms", true, @"Extraordinarily long arms");
            yield return new BodyFeature(this, @"Palms", false, @"Round pads on gripping surface");
            yield return new BodyFeature(this, @"Feet", false, @"Round pads on bottom");
            yield break;
        }

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // improved initiative
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new ImprovedInitiativeFeat(this, 1));

            // tentacle attacks (use 1d4 progression, making the small choker have 1d3)
            var _tentacle1 = new Tentacle(@"1d4", Size.Tiny, 20, 2, ItemSlot.HoldingSlot, @"Main", true,
                Contracts.DamageType.Bludgeoning, true);
            yield return new ExtraordinaryTrait(this, @"Tentacle", @"Attack with tentacle", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _tentacle1));

            var _tentacle2 = new Tentacle(@"1d4", Size.Tiny, 20, 2, ItemSlot.HoldingSlot, @"Off", true,
                Contracts.DamageType.Bludgeoning, true);
            yield return new ExtraordinaryTrait(this, @"Tentacle", @"Attack with tentacle", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _tentacle2));

            // TODO: improved grab
            // TODO: constrict (block verbal spell casting)
            // TODO: supernatural trait: quickness (OnRegisterActivity: extensibility/monitor; reset regular budget 1/round)

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
    }
}
