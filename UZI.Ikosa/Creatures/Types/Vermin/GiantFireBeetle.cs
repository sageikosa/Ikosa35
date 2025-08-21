using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class GiantFireBeetle : Species
    {
        #region static state
        private readonly static Dictionary<int, Roller> _BiteRollers;
        private readonly static List<SizeRange> _SizeRanges;
        #endregion

        static GiantFireBeetle()
        {
            _SizeRanges =
            [
                new SizeRange(1, 3, Size.Small, 1)
            ];
            _BiteRollers = WeaponDamageRollers.BuildRollerProgression(
                @"1d3", @"1d4", @"1d6", @"2d4", @"2d6", @"3d6", @"4d6", @"6d6", @"8d6");
        }

        public override Species TemplateClone(Creature creature)
            => new GiantFireBeetle();

        /// <summary>Giant fire beetles are not characters</summary>
        public override bool IsCharacterCapable => false;

        #region public override AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a tiny centipede
            var _set = new AbilitySet(10, 11, 11, 1, 10, 7);
            _set.Intelligence.IsNonAbility = true;
            return _set;
        }
        #endregion

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => null;    // never gets enough power dice

        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
            => null;    // no intelligence, no feats

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        #region protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        {
            // need to watch power die changes to selectively remove or re-add bonus feat
            var _class = new VerminClass<GiantFireBeetle>(new Type[] { }, 3, _SizeRanges, FractionalPowerDie, SmallestPowerDie, false);
            return _class;
        }
        #endregion

        #region protected override BodyType.Body GenerateBody()
        protected override Body GenerateBody()
            => new BeetleBody(Creature, Size.Small, 1)
            {
                BaseHeight = 2.5,
                BaseWidth = 2.5,
                BaseLength = 2.5
            };
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Body", true, @"Round hard outer shell");
            yield return new BodyFeature(this, @"Mouth", false, @"Pincer-like Fangs");
            yield return new BodyFeature(this, @"Legs", true, @"Six segmented legs");
            yield break;
        }
        #endregion

        protected override CreatureType GenerateCreatureType()
            => new VerminType();

        protected override int GenerateNaturalArmor()
            => 5;

        #region protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return Advancement.PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        {
            yield break;
        }

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NONE: giant fire beetles have no skills
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // Immune mind-affecting (descriptor) and death attacks (descriptor)
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(MindAffecting));
            yield return new ExtraordinaryTrait(this, @"Mindless Immunities", @"Mind-affecting",
                 TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // bite attack
            var _bite = new Bite(@"2d4", Size.Tiny, _BiteRollers, 20, 2, true, Contracts.DamageType.All, true);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            yield return new ExtraordinaryTrait(this, @"Gland", @"Gland mount", TraitCategory.Quality,
                new ItemSlotTrait(this, @"Gland", @"Left", false, false));
            yield return new ExtraordinaryTrait(this, @"Gland", @"Gland mount", TraitCategory.Quality,
                new ItemSlotTrait(this, @"Gland", @"Right", false, false));

            yield return new ExtraordinaryTrait(this, @"Glow gland", @"Localized Light", TraitCategory.Quality,
                new NaturalItemTrait(this, new GlowGland { Possessor = Creature }, @"Gland", false, new ActionTime(Minute.UnitFactor)));
            yield return new ExtraordinaryTrait(this, @"Glow gland", @"Localized Light", TraitCategory.Quality,
                new NaturalItemTrait(this, new GlowGland { Possessor = Creature }, @"Gland", false, new ActionTime(Minute.UnitFactor)));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
    }

    [Serializable]
    public class GlowGland : SlottedItemBase, ICloneable
    {
        public GlowGland()
            : base(@"Glow gland", @"Gland")
        {
            Price.CorePrice = 1m;
            BaseWeight = 0.5d;
            ItemMaterial = HideMaterial.Static;
            MaxStructurePoints.BaseValue = HideMaterial.Static.StructurePerInch * 2;
            var _illuminate = new Illumination(this, 10, 20, false);
            AddAdjunct(_illuminate);
        }

        public override bool IsTransferrable => false;
        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Free);
        public override ActionTime UnslottingTime => null;

        public override bool SlottingProvokes => false;
        public override bool UnslottingProvokes => true;

        public object Clone()
            => new GlowGland();

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            // making it a little bit more variable on ending time
            var _time = ((Setting as LocalMap)?.CurrentTime ?? 0)
                + Day.UnitFactor * DieRoller.RollDie(null, 5, @"Light Shed", @"Days")
                + Hour.UnitFactor * DieRoller.RollDie(null, 24, @"Light Shed", @"Hours");

            // expiry (NOTE: adding expiry to an existing adjunct) 
            AddAdjunct(new Expiry(Adjuncts.OfType<Illumination>().FirstOrDefault(_i => _i.Source == this),
                _time, TimeValTransition.Leaving, Hour.UnitFactor));

            base.OnClearSlots(slotA, slotB);
        }
    }
}
