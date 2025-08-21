using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class AnimatedObject : Species
    {
        #region public enum BodyForm { }
        public enum BodyForm
        {
            General,
            /// <summary>flexible sheetlike object: blinding grapple, constriction grapple and clumsy flight</summary>
            Sheet,
            /// <summary>flexible sinuous object: blinding grapple, constriction grapple and clumsy flight</summary>
            Coilable,
            /// <summary>Two legs with walking power: improved speed</summary>
            BiPedal,
            /// <summary>Four legs (or more) with walking power: improved speed</summary>
            QuadraPedal,
            /// <summary>Wheels: improved speed</summary>
            Roller
        }
        #endregion

        #region private data
        private Material _BodyMaterial = WoodMaterial.Static;
        private BodyForm _BodyForm = BodyForm.General;
        #endregion

        #region size range and Slam damage
        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _SlamRollers;

        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static AnimatedObject()
        {
            // non-standard size range adjustments for animated objects
            _SizeRanges =
            [
                new CustomSizeRange(0, 0, Size.Tiny, 0, 0, 0, 0, 0, 0),
                new CustomSizeRange(1, 1, Size.Small, 1, 1, -2, 2, 0, 2),
                new CustomSizeRange(2, 3, Size.Medium, 1, 1, -2, 2, 0, 2),
                new CustomSizeRange(4, 7, Size.Large, 2, 1, 0, 4, 0, 1),
                new CustomSizeRange(8, 15, Size.Huge, 3, 2, -2, 4, 0, 1),
                new CustomSizeRange(16, 31, Size.Gigantic, 4, 3, -2, 4, 0, 2),
                new CustomSizeRange(32, 47, Size.Colossal, 6, 4, -2, 4, 0, 4)
            ];

            // non-standard slam progression
            _SlamRollers = WeaponDamageRollers.BuildRollerProgression(
                @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"2d8", @"4d6");
        }
        #endregion

        #region public override AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a tiny animated object
            var _set = new AbilitySet(8, 14, 10, 10, 1, 1);
            _set.Constitution.IsNonAbility = true;
            _set.Intelligence.IsNonAbility = true;
            return _set;
        }
        #endregion

        /// <summary>Animated Objects are not characters</summary>
        public override bool IsCharacterCapable { get { return false; } }

        /// <summary>Animated Objects do get get boosts</summary>
        public override bool SupportsAbilityBoosts { get { return false; } }

        /// <summary>The smallest Animated Object has 1/2 PowerDie</summary>
        public override decimal FractionalPowerDie { get { return 0.5m; } }

        public override Species TemplateClone(Creature creature)
            => new AnimatedObject();

        #region public override IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel)
        public override IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel)
        {
            if (powerDieLevel == 1)
            {
                // material
                yield return new AdvancementRequirement(new RequirementKey(@"Material"), @"Material",
                    @"Material needed to determine hardness", MaterialSupplier, MaterialSetter, MaterialChecker)
                {
                    CurrentValue = new Feature(_BodyMaterial.Name, string.Format(@"Hardness={0}", _BodyMaterial.Hardness.ToString()))
                };

                // body-form: flexible, sheetlike, 2-legs, 4-legs, roller
                yield return new AdvancementRequirement(new RequirementKey(@"Form"), @"Form",
                    @"Form for movement and attacks", FormSupplier, FormSetter, FormChecker)
                {
                    CurrentValue = new Feature(_BodyForm.ToString(), FormPairs().FirstOrDefault(_kvp => _kvp.Key == _BodyForm).Value)
                };
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<Advancement.Feature> Features(int level)
        public override IEnumerable<Advancement.Feature> Features(int level)
        {
            if (level == 1)
            {
                var _formPair = FormPairs().FirstOrDefault(_kvp => _kvp.Key == _BodyForm);
                yield return new Feature(string.Format(@"Material: {0}", _BodyMaterial.Name), string.Format(@"Hardness={0}", _BodyMaterial.Hardness.ToString()));
                yield return new Feature(string.Format(@"Form: {0}", _BodyForm.ToString()), _formPair.Value);
            }
            yield break;
        }
        #endregion

        #region Material Management

        #region private IEnumerable<IAdvancementOption> MaterialSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> MaterialSupplier(IResolveRequirement target, RequirementKey key)
        {
            AdvancementParameter<Material> _option(Material material)
                => new AdvancementParameter<Material>(target, material.Name, material.Hardness.ToString(), material);

            yield return _option(StoneMaterial.Static);
            yield return _option(IronMaterial.Static);
            yield return _option(SteelMaterial.Static);
            yield return _option(WoodMaterial.Static);
            yield return _option(HideMaterial.Static);
            yield return _option(LeatherMaterial.Static);
            yield return _option(ClothMaterial.Static);
            yield return _option(RopeMaterial.Static);
            yield return _option(GlassMaterial.Static);
            yield return _option(BoneMaterial.Static);
            yield break;
        }
        #endregion

        #region private bool MaterialSetter(RequirementKey key, IAdvancementOption advOption)
        private bool MaterialSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<Material> _materialOption)
            {
                // set body material
                BodyMaterial = _materialOption.ParameterValue;
                return BodyMaterial == _materialOption.ParameterValue;
            }
            return false;
        }
        #endregion

        #region private bool MaterialChecker(RequirementKey key)
        private bool MaterialChecker(RequirementKey key)
        {
            return (_BodyMaterial != null);
        }
        #endregion

        #region public Material BodyMaterial
        public Material BodyMaterial
        {
            get { return _BodyMaterial; }
            set
            {
                value ??= WoodMaterial.Static;
                if (value != _BodyMaterial)
                {
                    _BodyMaterial = value;
                    ReplaceBody();
                }
            }
        }
        #endregion

        #endregion

        #region Form Management

        #region private IEnumerable<IAdvancementOption> FormSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> FormSupplier(IResolveRequirement target, RequirementKey key)
        {
            AdvancementParameter<BodyForm> _option(BodyForm form, string description)
                => new AdvancementParameter<BodyForm>(target, form.ToString(), description, form);

            foreach (var _kvp in FormPairs())
            {
                yield return _option(_kvp.Key, _kvp.Value);
            }

            yield break;
        }
        #endregion

        #region private IEnumerable<KeyValuePair<BodyForm, string>> FormPairs()
        private IEnumerable<KeyValuePair<BodyForm, string>> FormPairs()
        {
            yield return new KeyValuePair<BodyForm, string>(BodyForm.General, @"General Object");
            yield return new KeyValuePair<BodyForm, string>(BodyForm.Sheet, @"blinding grapple, constriction grapple and clumsy flight");
            yield return new KeyValuePair<BodyForm, string>(BodyForm.Coilable, @"blinding grapple, constriction grapple and clumsy flight");
            yield return new KeyValuePair<BodyForm, string>(BodyForm.BiPedal, @"improved speed +10");
            yield return new KeyValuePair<BodyForm, string>(BodyForm.QuadraPedal, @"improved speed +20");
            yield return new KeyValuePair<BodyForm, string>(BodyForm.Roller, @"improved speed +40");
            yield break;
        }
        #endregion

        #region private bool FormSetter(RequirementKey key, IAdvancementOption advOption)
        private bool FormSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<BodyForm> _formOption)
            {
                // set body form
                Form = _formOption.ParameterValue;
                return Form == _formOption.ParameterValue;
            }
            return false;
        }
        #endregion

        #region private bool FormChecker(RequirementKey key)
        private bool FormChecker(RequirementKey key)
        {
            return true;
        }
        #endregion

        #region public BodyForm Form
        public BodyForm Form
        {
            get { return _BodyForm; }
            set
            {
                if (value != _BodyForm)
                {
                    _BodyForm = value;
                    ReplaceBody();
                }
            }
        }
        #endregion

        #endregion

        #region private void ReplaceBody()
        private void ReplaceBody()
        {
            if (!_BodyMaterial.Equals(Creature.Body.BodyMaterial)
                || (_BodyForm != ((AnimatedObjectBody)Creature.Body).BodyForm))
            {
                // get rid of old group
                Creature.HeldItemsGroups.Remove(@"Natural");

                // get rid of old slam
                foreach (var _trait in (from _t in Creature.Traits
                                        where _t.Trait is NaturalWeaponTrait
                                        && _t.Source == this
                                        select _t).ToList())
                {
                    _trait.Eject();
                }

                // get size, since the creature may have been sized
                var _size = Creature.Body.Sizer.Size;

                // swap bodies
                var _newBody = new AnimatedObjectBody(Creature, _BodyMaterial, _BodyForm)
                {
                    BaseHeight = 1.25,
                    BaseWidth = 1.25,
                    BaseLength = 1.25
                };

                // new natural weapons
                var _hasNatural = false;
                foreach (var _natrl in GenerateTraits().Where(_t => _t.Trait is NaturalWeaponTrait))
                {
                    _hasNatural = true;
                    Creature.AddAdjunct(_natrl);
                }

                // features
                foreach (var _feature in GenerateBodyFeatures())
                {
                    _newBody.Features.Add(_feature);
                }

                // new natural armor
                _newBody.NaturalArmor.BaseValue = GenerateNaturalArmor();
                _newBody.BindTo(Creature);

                // apply new size
                _newBody.Sizer.Size = _size;

                if (_hasNatural)
                {
                    // snapshot
                    Creature.HeldItemsGroups.Snapshot(@"Natural");
                }
            }
        }
        #endregion

        #region protected override CreatureType GenerateCreatureType()
        protected override CreatureType GenerateCreatureType()
        {
            return new ConstructType();
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override BodyType.Body GenerateBody()
        protected override BodyType.Body GenerateBody()
        {
            // determined by requirements
            return new AnimatedObjectBody(Creature, WoodMaterial.Static, BodyForm.General)
            {
                BaseHeight = 1.25,
                BaseWidth = 1.25,
                BaseLength = 1.25
            };
        }
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            // determined by requirements
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        {
            // varies by sub-animated type?
            // NOTE: land-movement is sourced by the body in this case, as speed varies with size
            yield break;
        }
        #endregion

        #region protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        {
            return new ConstructClass<AnimatedObject>(new Type[] { }, 47, _SizeRanges, FractionalPowerDie, SmallestPowerDie, true);
        }
        #endregion

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return Advancement.PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        {
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            // no ability boosting
            return null;
        }
        #endregion

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            // no speaky
            yield break;
        }
        #endregion

        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // varies non-uniformly by size
            return 0;
        }
        #endregion

        // TODO: factor this into ConstructSpecies class
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // slam attack
            var _slam = new Slam(@"1d3", Size.Medium, _SlamRollers, 20, 2, true, DamageType.Bludgeoning, true)
            {
                ItemMaterial = _BodyMaterial
            };
            _slam.MainHead.HeadMaterial = _BodyMaterial;
            yield return new ExtraordinaryTrait(this, @"Slam", @"Attack with body", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _slam));

            // Immune to poison, sleep, paralysis, stunning, disease, fatigue, exhaustion
            var _immune = new MultiAdjunctBlocker(this, @"Immune",
                typeof(Poisoned), typeof(SleepEffect), typeof(UnconsciousEffect), typeof(StunnedEffect),
                typeof(Fatigued), typeof(Exhausted), typeof(ParalyzedEffect), typeof(Diseased)
                );
            yield return new ExtraordinaryTrait(this, @"Construct Immunities",
                @"Poison, sleep, stun, paralysis, disease, fatigue, exhaustion",
                 TraitCategory.Quality, new AdjunctTrait(this, _immune));

            // Immune mind-affecting (descriptor) and death attacks (descriptor)
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(MindAffecting), typeof(Death));
            yield return new ExtraordinaryTrait(this, @"Construct Immunities", @"Mind-affecting and death attacks",
                 TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // Immune necromancy (magic-style)
            var _styleBlock = new MagicStyleBlocker<Necromancy>(this);
            yield return new ExtraordinaryTrait(this, @"Construct Immunities", @"Necromancy",
                TraitCategory.Quality, new AdjunctTrait(this, _styleBlock));

            // Cannot naturally heal, nor be healed
            yield return new ExtraordinaryTrait(this, @"Unhealable", @"Cannot be healed", TraitCategory.Quality,
                new InteractHandlerTrait(this, new CreatureNoRecoverHealthPointHandler()));

            // Immune to critical hits
            yield return new ExtraordinaryTrait(this, @"Immune critical", @"Critical ignore chance 100%", TraitCategory.Quality,
                new InteractHandlerTrait(this, new CriticalFilterHandler(100)));

            // Immune to nonlethal damage
            // Destroyed when reduced to 0 health points or less
            yield return new ExtraordinaryTrait(this, @"Immune to non-lethal", @"Non-lethal is ignored", TraitCategory.Quality,
                new InteractHandlerTrait(this, new CreatureNonLivingDamageHandler()));

            // hardness reduces damage
            yield return new ExtraordinaryTrait(this, @"Hardness", @"Hardness reduces damage", TraitCategory.Quality,
                new InteractHandlerTrait(this, new HardnessDamageReducer()));

            // Cannot be raised nor resurrected

            // Constructs do not eat, sleep, or breathe
            // TODO: many from construct creature type
            yield break;
        }
    }
}
