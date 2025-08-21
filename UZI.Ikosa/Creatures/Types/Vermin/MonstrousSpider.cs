using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class MonstrousSpider : Species, IPoisonProvider, IMonitorChange<int>, ITraitPowerClassSource
    {
        public override Species TemplateClone(Creature creature)
            => new MonstrousSpider();

        #region private data
        private SpiderForm _SpiderForm = SpiderForm.Hunter;
        #endregion

        #region size range, bite damage, and ¿poison damage?
        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _BiteRollers;

        private readonly static List<SizeRange> _SizeRanges;

        static MonstrousSpider()
        {
            // non-standard size range adjustments for spiders
            _SizeRanges =
            [
                new CustomSizeRange(0, 0, Size.Tiny, 0, 0, 0, 0, 0, 0),
                new CustomSizeRange(1, 1, Size.Small, 1, 1, 0, 4, 0, 0),
                new CustomSizeRange(2, 3, Size.Medium, 1, 1, 0, 4, 2, 1),
                new CustomSizeRange(4, 7, Size.Large, 1, 1, 0, 4, 0, 1),
                new CustomSizeRange(8, 15, Size.Huge, 2, 2, 0, 4, 2, 3),
                new CustomSizeRange(16, 31, Size.Gigantic, 3, 3, 0, 6, 0, 5),
                new CustomSizeRange(32, 47, Size.Colossal, 6, 6, -2, 6, 0, 8)
            ];

            // non-standard bite progression
            _BiteRollers = WeaponDamageRollers.BuildRollerProgression(
                @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"2d8", @"4d6");
        }
        #endregion

        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a tiny spider
            var _set = new AbilitySet(3, 17, 10, 1, 10, 2);
            _set.Intelligence.IsNonAbility = true;
            return _set;
        }
        #endregion

        /// <summary>Spiders are not characters</summary>
        public override bool IsCharacterCapable { get { return false; } }

        /// <summary>Spiders do get get boosts</summary>
        public override bool SupportsAbilityBoosts { get { return false; } }

        /// <summary>The smallest spider gets 1/2 PowerDie</summary>
        public override decimal FractionalPowerDie { get { return 0.5m; } }

        #region public override IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel)
        public override IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel)
        {
            if (powerDieLevel == 1)
            {
                // spider form
                yield return new AdvancementRequirement(new RequirementKey(@"SpiderForm"), @"Spider-Form",
                    @"Hunter or web-spinner", SpiderFormSupplier, SpiderFormSetter, SpiderFormChecker)
                {
                    CurrentValue = Features(1).FirstOrDefault()
                };
            }
            yield break;
        }
        #endregion

        #region spider form management

        #region private IEnumerable<IAdvancementOption> SpiderFormSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> SpiderFormSupplier(IResolveRequirement target, RequirementKey key)
        {
            yield return new AdvancementParameter<SpiderForm>
                (target, @"Hunter", @"Faster and jumpier, but no web as weapon", SpiderForm.Hunter);
            yield return new AdvancementParameter<SpiderForm>
                (target, @"Web-Spinner", @"Web as weapon to entangle", SpiderForm.WebSpinner);
            yield break;
        }
        #endregion

        #region private bool SpiderFormSetter(RequirementKey key, IAdvancementOption advOption)
        private bool SpiderFormSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<SpiderForm> _formOption)
            {
                // set spider form
                Form = _formOption.ParameterValue;
                return true;
            }
            return false;
        }
        #endregion

        #region private bool SpiderFormChecker(RequirementKey key)
        private bool SpiderFormChecker(RequirementKey key)
        {
            return true;
        }
        #endregion

        #region public SpiderForm Form
        public SpiderForm Form
        {
            get { return _SpiderForm; }
            set
            {
                if (_SpiderForm == SpiderForm.WebSpinner)
                {
                    // spider web net throw (if defined)
                    Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                        .Where(_et => (_et.Trait is SpiderWebNetReload _swnr) && (_swnr.PowerClass == PowerClass))
                        .FirstOrDefault()?.Eject();
                }

                if (value != _SpiderForm)
                {
                    // replace body
                    _SpiderForm = value;
                    ReplaceBody();
                }

                if (_SpiderForm == SpiderForm.WebSpinner)
                {
                    var _reload = new SpiderWebNetReload(this);
                    Creature.AddAdjunct(new ExtraordinaryTrait(_reload.TraitSource, @"Web Net Reload", @"Reload spinneret with a throwable web (8/day)",
                        TraitCategory.AttackMode, _reload));
                }
            }
        }
        #endregion

        #region private void ReplaceBody()
        private void ReplaceBody()
        {
            if (Form != ((SpiderBody)Creature.Body).SpiderForm)
            {
                // get rid of old group
                Creature.HeldItemsGroups.Remove(@"Natural");

                // get rid of old bite
                foreach (var _trait in (from _t in Creature.Traits
                                        where _t.Trait is NaturalWeaponTrait
                                        && _t.Source == this
                                        select _t).ToList())
                {
                    _trait.Eject();
                }

                // get size, since the creature may have been sized
                var _size = Creature.Body.Sizer.NaturalSize;

                // swap bodies
                var _newBody = GenerateBody();

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
                _newBody.Sizer.NaturalSize = _size;

                if (_hasNatural)
                {
                    // snapshot
                    Creature.HeldItemsGroups.Snapshot(@"Natural");
                }
            }
        }
        #endregion

        #endregion

        #region public override IEnumerable<Advancement.Feature> Features(int level)
        public override IEnumerable<Advancement.Feature> Features(int level)
        {
            if (level == 1)
            {
                yield return new Feature(_SpiderForm.ToString(),
                                _SpiderForm == SpiderForm.Hunter
                                ? @"+10 land speed, +10 jump, +8 spot"
                                : @"Web as net, +8 Hide, +8 Silent Stealth"
                                );
            }
            yield break;
        }
        #endregion

        #region protected override CreatureType GenerateCreatureType()
        protected override CreatureType GenerateCreatureType()
        {
            return new VerminType();
        }
        #endregion

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
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

        #region protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        {
            return null;
        }
        #endregion

        #region protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        {
            // need to watch power die changes to selectively remove or re-add bonus feat
            var _class = new VerminClass<MonstrousSpider>(new Type[] { }, 47, _SizeRanges, FractionalPowerDie, SmallestPowerDie, false);
            _class.AddChangeMonitor(this);
            return _class;
        }
        #endregion

        public IPowerClass PowerClass
            => Creature.Classes.OfType<VerminClass<MonstrousSpider>>().FirstOrDefault();

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new SpiderBody(Creature, Form)
            {
                BaseHeight = 1.25,
                BaseWidth = 1.25,
                BaseLength = 1.25,
                BaseWeight = 5
            };
        }
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Body", true, @"Rigid Tough Body");
            yield return new BodyFeature(this, @"Eyes", false, @"Compound Eyes");
            yield return new BodyFeature(this, @"Mouth", false, @"Pincer-like Fangs");
            yield return new BodyFeature(this, @"Legs", true, @"8 Legs");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        {
            // NOTE: land-movement is sourced by the body, as speed varies with size and type
            // NOTE: climb-movement varies by size
            yield break;
        }
        #endregion

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // varies non-uniformly by size
            return 0;
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
            yield return new Tremorsense(60, this);
            // TODO: web-sense?
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NONE: spiders have bonuses and ability modifiers to specific things
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        {
            // +4 Hide/Spot
            var _skill4 = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill4);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill4);

            // +8 Climb
            var _skill8 = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill8);
            yield break;
        }
        #endregion

        protected override IEnumerable<Adjuncts.TraitBase> GenerateTraits()
        {
            // Web-Spinners: +8 Stealth (with webs)
            // Web-Spinners: +8 Silent Stealth (with webs)
            // TODO: spider traits

            // better of STR/DEX for Climb
            var _better = new BetterDelta(Creature.Abilities.Ability<Dexterity>(), Creature.Abilities.Ability<Strength>());
            yield return new ExtraordinaryTrait(this, @"Nimble Climber", @"Uses Dexterity for Climb if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<ClimbSkill>()));

            // Immune mind-affecting (descriptor) and death attacks (descriptor)
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(MindAffecting));
            yield return new ExtraordinaryTrait(this, @"Mindless Immunities", @"Mind-affecting",
                 TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // poisonous
            var _poisonTrait = new PoisonTrait(this, new Poisonous(this));
            yield return new ExtraordinaryTrait(this, @"Poison", @"Poisonous Bite", TraitCategory.CombatHelper,
                _poisonTrait);

            // bite attack
            var _bite = new Bite(@"1d3", Size.Tiny, _BiteRollers, 20, 2, true, Contracts.DamageType.All, true);
            var _wpnPoison = new WeaponSecondarySpecialAttackResult(_poisonTrait, false, true)
            {
                PoweredUp = true
            };
            _bite.MainHead.AddAdjunct(_wpnPoison);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // weapon finesse (bonus feat)
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // web burst boost
            yield return new ExtraordinaryTrait(this, @"Tough Web", @"+4 to Burst Web", TraitCategory.Quality,
                new QualifyDeltaTrait(this, new SpiderWebBoost(), Creature.ExtraClassPowerLevel));

            yield break;
        }

        #region public Poison GetPoison()
        public Poison GetPoison()
        {
            // poison damage by size
            Roller _roller()
            {
                switch (Creature.Body.Sizer.Size.Order)
                {
                    case 4:  // colossal
                        return new DiceRoller(2, 8);

                    case 3:  // gigantic
                        return new DiceRoller(2, 6);

                    case 2:  // huge
                        return new DieRoller(8);

                    case 1:  // large
                        return new DieRoller(6);

                    case 0:  // medium
                        return new DieRoller(4);

                    case -1: // small
                        return new DieRoller(3);

                    case -2: // Tiny
                    default:
                        return new DieRoller(2);
                };
            }
            var _dmg = new AbilityPoisonDamage(MnemonicCode.Str, _roller());

            // build poison
            var _class = Creature.Classes.Get<VerminClass<MonstrousSpider>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Con, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Spider Poison", _dmg, _dmg,
                Poison.ActivationMethod.Injury,
                Poison.MaterialForm.Liquid,
                _difficulty);
        }
        #endregion

        #region IMonitorChange<int> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            if ((args.OldValue >= 4) && (args.NewValue < 4))
            {
                // going back to medium...re-add weapon finesse
                Creature.AddAdjunct(BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                   new WeaponFinesse(this, 1) { IgnorePreRequisite = true }));
            }
            else if ((args.OldValue < 4) && (args.NewValue >= 4))
            {
                // going to large...remove weapon finesse
                var _trait = (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                              where _ex.Source == this
                              let _feat = _ex.Trait as BonusFeatTrait
                              where _feat != null && _feat.Feat is WeaponFinesse
                              select _ex).FirstOrDefault();
                if (_trait != null)
                {
                    _trait.Eject();
                }
            }
        }

        #endregion
    }

    [Serializable]
    public class SpiderWebBoost : IQualifyDelta
    {
        public SpiderWebBoost()
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(4, typeof(Racial), @"Spider Web Boost");
        }

        private readonly IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((((Type)qualify.Source) == typeof(SpiderWebNet))
                && qualify.Target is SpiderWebNet)
            {
                yield return _Delta;
            }
            yield break;
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
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

    [Serializable]
    public enum SpiderForm
    {
        Hunter,
        WebSpinner
    }

    [Serializable]
    public class SpiderWebNetReload : TraitEffect, IActionProvider, IActionSource
    {
        public SpiderWebNetReload(ITraitSource traitSource)
            : base(traitSource)
        {
            _WebBattery = new RegeneratingBattery(this, 8, Day.UnitFactor);
        }

        #region data
        private RegeneratingBattery _WebBattery;
        #endregion

        public RegeneratingBattery Battery => _WebBattery;
        public IPowerClass PowerClass => (TraitSource as ITraitPowerClassSource)?.PowerClass;

        public IVolatileValue ActionClassLevel 
            => PowerClass?.ClassPowerLevel ?? Creature.ActionClassLevel;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.AddAdjunct(_WebBattery);
            Creature?.Actions.Providers.Add(this, this);
        }

        protected override void OnDeactivate(object source)
        {
            Creature?.Actions.Providers.Remove(this);
            _WebBattery.Eject();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new SpiderWebNetReload(TraitSource);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new SpiderWebNetReload(traitSource);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Battery.CanUseCharges(1)
                && (Creature.Body is SpiderBody _spider)
                && (_spider.SpiderForm == SpiderForm.WebSpinner)
                && _spider.ItemSlots.Contains(ItemSlot.Spinneret)
                && (_spider.ItemSlots[ItemSlot.Spinneret].SlottedItem == null))
            {
                yield return new ReloadSpiderWebNet(this, @"201");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return new AdjunctInfo(@"Load Spinneret with throwable web", ID);
        }
    }

    [Serializable]
    public class ReloadSpiderWebNet : ActionBase
    {
        public ReloadSpiderWebNet(SpiderWebNetReload reloader, string orderKey)
            : base(reloader, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public SpiderWebNetReload SpiderWebNetReload => ActionSource as SpiderWebNetReload;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override string Key => @"Web.Reload";
        public override string DisplayName(CoreActor actor) => @"Reload web net";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // implicitly the first unloaded spinneret found on the body
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (SpiderWebNetReload.Battery.CanUseCharges(1))
            {
                var _net = new SpiderWebNet(SpiderWebNetReload.Creature, SpiderWebNetReload.PowerClass, 5);
                var _slot = SpiderWebNetReload.Creature.Body.ItemSlots.AvailableSlots(_net).FirstOrDefault();
                if (_slot != null)
                {
                    _net.Possessor = SpiderWebNetReload.Creature;
                    _net.SetItemSlot(_slot);
                    SpiderWebNetReload.Battery.UseCharges(1);
                }
            }
            return new RegisterActivityStep(activity, Budget);
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Reload web", activity.Actor, observer);
        }
    }
}
