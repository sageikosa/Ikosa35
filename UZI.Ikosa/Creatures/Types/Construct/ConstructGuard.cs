using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class ConstructGuard : Species, IWeaponProficiency, IArmorProficiency, IShieldProficiency, IMonitorChange<int>
    {
        #region data
        private Language _Lang;
        private Material _BodyMaterial = IronMaterial.Static;
        private Delta _Fire;
        private Delta _Cold;
        #endregion

        #region size range
        // make sure the AnimatedObject's size progression is setup
        private readonly static List<SizeRange> _SizeRanges;

        static ConstructGuard()
        {
            _SizeRanges =
            [
                new SizeRange(1, 10, Size.Medium, 1),
                new SizeRange(11, 15, Size.Large, 2)
            ];
        }
        #endregion

        #region public override AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a medium guard
            var _set = new AbilitySet(16, 11, 1, 6, 13, 2);
            _set.Constitution.IsNonAbility = true;
            return _set;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new ConstructGuard();

        /// <summary>Not characters</summary>
        public override bool IsCharacterCapable => false;

        /// <summary>Do get boosts</summary>
        public override bool SupportsAbilityBoosts => true;

        /// <summary>No fractional PowerDie</summary>
        public override decimal FractionalPowerDie => 1m;

        public override IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel)
        {
            if (powerDieLevel == 1)
            {
                // language
                yield return new AdvancementRequirement(new RequirementKey(@"Language"), @"Language",
                    @"Language understood by guard", LanguageSupplier, LanguageSetter, LanguageChecker)
                {
                    CurrentValue = new Feature(_Lang.Name, $@"Can understand {_Lang.Name}")
                };
            }
            // TODO: perhaps make energy resistances selectable?
            yield break;
        }

        public override IEnumerable<Feature> Features(int level)
        {
            if (level == 1)
            {
                yield return new Feature(_Lang.Name, $@"Can understand {_Lang.Name}");
            }
            yield break;
        }

        #region Language Management

        #region private IEnumerable<IAdvancementOption> LanguageSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> LanguageSupplier(IResolveRequirement target, RequirementKey key)
        {
            AdvancementParameter<Language> _option(Language language)
                => new AdvancementParameter<Language>(target, language.Name, language.Name, language);

            yield return _option(new Abyssal(this));
            yield return _option(new Aquan(this));
            yield return _option(new Auran(this));
            yield return _option(new Celestial(this));
            yield return _option(new Common(this));
            yield return _option(new Draconic(this));
            yield return _option(new Druidic(this));
            yield return _option(new Dwarven(this));
            yield return _option(new Elven(this));
            yield return _option(new Giant(this));
            yield return _option(new Languages.Gnome(this));
            yield return _option(new Gnollish(this));
            yield return _option(new Grimlockese(this));
            yield return _option(new Languages.Halfling(this));
            yield return _option(new Ignan(this));
            yield return _option(new Infernal(this));
            yield return _option(new Orcish(this));
            yield return _option(new Sylvan(this));
            yield return _option(new Terran(this));
            yield return _option(new Undercommon(this));
            yield break;
        }
        #endregion

        #region private bool LanguageSetter(RequirementKey key, IAdvancementOption advOption)
        private bool LanguageSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<Language> _languageOption)
            {
                // set language
                Creature.Languages.Remove(_Lang);
                _Lang = new MuteLanguage(_languageOption.ParameterValue);
                Creature.Languages.Add(_Lang);
                return true;
            }
            return false;
        }
        #endregion

        #region private bool LanguageChecker(RequirementKey key)
        private bool LanguageChecker(RequirementKey key)
        {
            return (_Lang != null);
        }
        #endregion

        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType != WeaponProficiencyType.Natural);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => true;

        string IWeaponProficiency.Description => @"Weapon wielded";

        #endregion

        protected override CreatureType GenerateCreatureType()
            => new ConstructType();

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override BodyType.Body GenerateBody()
        protected override Body GenerateBody()
        {
            // determined by requirements
            return new HumanoidBody(IronMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2,
                BaseLength = 2,
                BaseWeight = 120
            };
        }
        #endregion

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield break;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            // TODO: disable run...
            var _land = new LandMovement(20, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ConstructClass<ConstructGuard>(new Type[] { typeof(SpotSkill) }, 15, _SizeRanges,
                FractionalPowerDie, SmallestPowerDie, false);
            _class.AddChangeMonitor(this);
            return _class;
        }

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield return PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new PowerAttackFeat(powerDie, 1);
                case 3:
                    return new CleaveFeat(powerDie, 3);
            }
            return null;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _skills = new[] { typeof(SpotSkill) };
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _skills, new int[] { 1 });
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
            if ((powerDieLevel % 4) == 0)
            {
                return MnemonicCode.Str;
            }

            return null;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            _Lang = new MuteLanguage(new Common(this));
            yield return _Lang;
            yield break;
        }
        #endregion

        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(false, this);
            yield return new Hearing(this);
            yield break;
        }

        protected override int GenerateNaturalArmor()
            => 0;

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // weapon proficiency
            yield return new ExtraordinaryTrait(this, @"Weapon Proficiency", @"Proficient with wielded weapon",
                TraitCategory.Quality, new WrappedWeaponProficiencyTrait(this, this));

            // armor proficiency
            yield return new ExtraordinaryTrait(this, @"Armor Proficiency", @"Proficient with donned armor",
                TraitCategory.Quality, new WrappedArmorProficiencyTrait(this, this));

            // shield proficiency
            yield return new ExtraordinaryTrait(this, @"Shield Proficiency", @"Proficient with shield",
                TraitCategory.Quality, new WrappedShieldProficiencyTrait(this, this));

            // Immune to poison, sleep, paralysis, stunning, disease, fatigue, exhaustion
            var _immune = new MultiAdjunctBlocker(this, @"Immune",
                typeof(Poisoned), typeof(SleepEffect), typeof(UnconsciousEffect), typeof(StunnedEffect),
                typeof(Fatigued), typeof(Exhausted), typeof(ParalyzedEffect), typeof(Diseased)
                );
            yield return new ExtraordinaryTrait(this, @"Construct Immunities",
                @"Poison, sleep, stun, paralysis, disease, fatigue, exhaustion",
                 TraitCategory.Quality, new AdjunctTrait(this, _immune));

            // cannot run
            var _slowPoke = new MultiActionFilter(this, @"Cannot Run", typeof(StartRun));
            yield return new ExtraordinaryTrait(this, @"Cannot Run", @"Cannot use start run action",
                 TraitCategory.Quality, new AdjunctTrait(this, _slowPoke));

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

            // Cannot be raised nor resurrected

            // Constructs do not eat, sleep, or breathe
            // TODO: many from construct creature type
            yield break;
        }
        #endregion

        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
            => true;

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => true;

        string IArmorProficiency.Description
            => @"Armor worn";

        public bool IsProficientWithShield(bool tower, int powerLevel)
            => true;

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => true;

        string IShieldProficiency.Description
            => @"Shield held";

        #region IMonitorChange<int> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            // what should it be?
            var _calc = 0;
            if (args.NewValue > 10)
            {
                _calc = 15;
            }
            else if (args.NewValue > 4)
            {
                _calc = 10;
            }
            else if (args.NewValue > 2)
            {
                _calc = 5;
            }

            // adjust if necessary
            if (_Fire == null)
            {
                if (_calc != 0)
                {
                    _Fire = new Delta(_calc, typeof(EnergyResistance));
                    _Cold = new Delta(_calc, typeof(EnergyResistance));
                }
            }
            else if (_Fire.Value != _calc)
            {
                _Fire.Value = _calc;
                _Cold.Value = _calc;
            }

            if ((args.OldValue < 3) && (args.NewValue >= 3))
            {
                // resist fire damage (10)
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Fire Resistance", @"Resist to fire",
                    TraitCategory.Quality, new DeltaTrait(this, _Fire, Creature.EnergyResistances[EnergyType.Fire])));
            }
            else if ((args.OldValue < 5) && (args.NewValue >= 5))
            {
                // resist cold damage (10)
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Cold Resistance", @"Resist to cold",
                    TraitCategory.Quality, new DeltaTrait(this, _Cold, Creature.EnergyResistances[EnergyType.Cold])));
            }
            else if ((args.OldValue >= 5) && (args.NewValue < 5))
            {
                foreach (var _trait in (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                                        where _ex.Source == this
                                        && _ex.Trait is DeltaTrait
                                        && _ex.Name == @"Cold Resistance"
                                        select _ex).ToList())
                {
                    _trait.Eject();
                }
            }
            else if ((args.OldValue >= 3) && (args.NewValue < 3))
            {
                foreach (var _trait in (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                                        where _ex.Source == this
                                        && _ex.Trait is DeltaTrait
                                        && _ex.Name == @"Fire Resistance"
                                        select _ex).ToList())
                {
                    _trait.Eject();
                }
            }
        }

        #endregion
    }
}
