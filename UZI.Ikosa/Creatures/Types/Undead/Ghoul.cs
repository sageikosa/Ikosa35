using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Visualize;
using System.Diagnostics;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Ghoul : Species, IReplaceCreature, IMonitorChange<int>, IStenchGeometryBuilderFactory,
        IParalysisProvider, IDiseaseProvider, IPoisonProvider // IMonitorChange<Level>
    {
        #region ctor()
        public Ghoul()
        {
        }

        public Ghoul(Creature original)
        {
            _Original = original;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new Ghoul(Original.TemplateClone(Name));

        private Creature _Original;
        public Creature Original => _Original;

        public string TemplateName => Name;

        public bool IsAcquired => true;

        #region protected override void OnConnectSpecies()
        protected override void OnConnectSpecies()
        {
            base.OnConnectSpecies();
            if (_Original != null)
            {
                UnslotAllItems(_Original);
            }
        }
        #endregion

        #region protected override void OnDisconnectSpecies()
        protected override void OnDisconnectSpecies()
        {
            if (_Original != null)
            {
                TransferItems(Creature, _Original);
            }
            base.OnDisconnectSpecies();
        }
        #endregion

        #region _ClassSkills
        // 20 + 5@2 (+ 10@4)
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(SpotSkill),
                typeof(BalanceSkill),
                typeof(ClimbSkill),
                typeof(StealthSkill),
                typeof(JumpSkill),
                typeof(SilentStealthSkill)
            };
        #endregion

        #region public override AbilitySet DefaultAbilities()
        // default ability set for making a ghoul
        public override AbilitySet DefaultAbilities()
        {
            var _set = new AbilitySet(13, 15, 10, 13, 14, 12);
            _set[MnemonicCode.Con].IsNonAbility = true;
            return _set;
        }
        #endregion

        public override string Name
            => (Creature.Classes.Get<UndeadClass<Ghoul>>()?.CurrentLevel >= 4)
                ? @"Ghast"
                : base.Name;

        public override bool IsCharacterCapable
            => false;

        protected override CreatureType GenerateCreatureType()
            => new UndeadType();

        #region protected override BaseMonsterClass GenerateBaseMonsterClass()
        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            // need to watch power die changes to alter traits
            var _class = new UndeadClass<Ghoul>(_ClassSkills, 8, 0m, 0m, false);
            _class.AddChangeMonitor(this);
            return _class;
        }
        #endregion

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => ((powerDieLevel % 4) == 0)
            ? MnemonicCode.Str
            : null;

        protected override int GenerateNaturalArmor()
            => 2;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
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
            yield return new BodyFeature(this, @"Skin", false, @"Mottled Loose Skin");
            yield return new BodyFeature(this, @"Stature", true, @"Hunched over");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            // A ghoul’s base land speed is 30 feet, unless based off a small creature
            var _land = (_Original == null)
                ? new LandMovement(30, Creature, this)
                : new LandMovement((Original.Sizer.NaturalSize.Order < Size.Medium.Order) ? 20 : 30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            if (_Original == null)
            {
                yield return PowerDieCalcMethod.Average;
                yield return PowerDieCalcMethod.Average;
            }
            else
            {
                // no more than 8 (but no less than 2)
                var _last = Math.Max(Math.Min(_Original.AdvancementLog.NumberPowerDice, 8), 2);
                for (var _px = 0; _px < _last; _px++)
                {
                    yield return PowerDieCalcMethod.Average;
                }
            }
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

                case 3:
                    return new ToughnessFeat(powerDie, 3);

                case 6:
                    return new ToughnessFeat(powerDie, 6);
            }
            return null;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _ClassSkills, new int[] { 1, 1, 1, 1, 1, 1 });
        }

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            if (_Original == null)
            {
                yield return new Common(this);
            }
            else
            {
                foreach (var _lang in GenerateLanguageCopies(_Original))
                {
                    yield return _lang;
                }
            }
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
            // alignment=chaotic evil
            var _aligned = new AlignedCreature(Alignment.ChaoticEvil);
            yield return new ExtraordinaryTrait(this, @"Alignment", @"Always chaotic evil",
                TraitCategory.Quality, new AdjunctTrait(this, _aligned));

            foreach (var _trait in UndeadType.UndeadPowerImmunities(this))
                yield return _trait;

            foreach (var _trait in UndeadType.UndeadEffectImmunities(this))
                yield return _trait;

            foreach (var _trait in UndeadType.UndeadUnhealth(this))
                yield return _trait;

            // ghoul needs item slots for secondary natural attacks
            yield return new ExtraordinaryTrait(this, @"Mouth Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                new ItemSlotTrait(this, ItemSlot.Mouth, string.Empty, false, false));

            // paralysis...
            var _paralysisTrait = new ParalysisTrait(this, this);
            yield return new ExtraordinaryTrait(this, @"Paralysis", @"Paralysis", TraitCategory.CombatHelper,
                _paralysisTrait);

            // disease...
            var _diseaseTrait = new DiseaseTrait(this, this);
            yield return new ExtraordinaryTrait(this, @"Disease", @"Ghoul Fever", TraitCategory.CombatHelper,
                _diseaseTrait);

            // and needs all the natural attacks added (with a magical enhancement of 0)
            var _claw = new Claw(@"1d3", Size.Miniature, 20, 2, @"Main", false, false);
            var _wpnParalysis = new WeaponSecondarySpecialAttackResult(_paralysisTrait, false, true);
            _claw.MainHead.AddAdjunct(_wpnParalysis);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw));

            var _claw2 = new Claw(@"1d3", Size.Miniature, 20, 2, @"Off", false, false);
            var _wpnParalysis2 = new WeaponSecondarySpecialAttackResult(_paralysisTrait, false, true);
            _claw2.MainHead.AddAdjunct(_wpnParalysis2);
            yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _claw2));

            var _bite = new Bite(@"1d6", Size.Tiny, 20, 2, true, false);
            var _wpnParalysis3 = new WeaponSecondarySpecialAttackResult(_paralysisTrait, false, true);
            _bite.MainHead.AddAdjunct(_wpnParalysis3);
            var _wpnDisease = new WeaponSecondarySpecialAttackResult(_diseaseTrait, false, true);
            _bite.MainHead.AddAdjunct(_wpnDisease);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // drive resistance +2
            yield return new ExtraordinaryTrait(this, @"Drive Resistance", @"+2 to Overwhelm, Repulse, Reinforce, or Dispel one of those effects",
                TraitCategory.Quality,
                new QualifyDeltaTrait(this, new DriveResistance<DriveUndeadAdjunct>(this, 2, @"Drive Resistance"),
                Creature.AdvancementLog.PowerLevel));
            yield break;
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
            if (_Original != null)
            {
                if (args.OldValue == 0)
                {
                    // sync size after natural weapons are added
                    Creature.Sizer.NaturalSize = Original.Sizer.NaturalSize;
                }
            }

            if ((args.OldValue >= 4) && (args.NewValue < 4))
            {
                // going from ghast back to ghoul
                foreach (var _trait in (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                                        where _ex.Source == this && _ex.Trait is DeltaTrait
                                        select _ex).ToList())
                {
                    _trait.Eject();
                }

                // Stench
                (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                 where _ex.Source == this && _ex.Trait is AdjunctTrait
                 let _at = _ex.Trait as AdjunctTrait
                 where _at.Adjunct is SpeciesStench<Ghoul>
                 select _ex)?.FirstOrDefault()?.Eject();
            }
            else if ((args.OldValue < 4) && (args.NewValue >= 4))
            {
                // going from ghoul to ghast
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Ghastly Strength", @"+3", TraitCategory.Quality,
                    new DeltaTrait(this, new Delta(3, this, @"Racial Trait"), Creature.Abilities.Strength)));
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Ghastly Dexterity", @"+2", TraitCategory.Quality,
                    new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity)));
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Ghastly Charisma", @"+4", TraitCategory.Quality,
                    new DeltaTrait(this, new Delta(4, this, @"Racial Trait"), Creature.Abilities.Charisma)));
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Ghastly Natural Armor", @"+2", TraitCategory.Quality,
                    new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Body.NaturalArmor)));

                // Stench
                //var _expectEven = _loc.LocationAimMode == LocationAimMode.Intersection;

                // define new capture
                //var _locSize = SourceSize(_loc);
                //var _stenchSize = new GeometricSize(
                //    _locSize.ZHeight + (Builder * 2),
                //    _locSize.YLength + (Builder * 2),
                //    _locSize.XLength + (Builder * 2));
                //var _reachZone = new CubicBuilder(_stenchSize, _stenchSize.CenterCell(_expectEven));
                Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Ghastly Stench", @"Sicken nearby creatures", TraitCategory.Quality,
                    new AdjunctTrait(this, new CreatureStenchControl(new SpeciesStench<Ghoul>(this, this), 10, false))));
            }
        }

        #endregion

        #region IParalysisProvider
        public IVolatileValue Difficulty =>
            Creature.GetIntrinsicPowerDifficulty(
                Creature.Classes.Get<UndeadClass<Ghoul>>(), MnemonicCode.Cha, typeof(ParalysisTrait));

        public object QualifierSource => this;
        public SaveType SaveType => SaveType.Fortitude;
        public Roller TimeUnits => new ComplexDiceRoller(@"1d4+1");
        public TimeUnit UnitFactor => new Round();

        /// <summary>
        /// ghoul paralysis does not affect elves, unless the ghoul is really a ghast
        /// </summary>
        public bool WillAffect(IInteract target)
            => (target is Creature _critter)
            && ((!(_critter?.Species is Elf)) || Creature.Classes.Get<UndeadClass<Ghoul>>().CurrentLevel > 3);
        #endregion

        #region IDiseaseProvider
        public Disease GetDisease()
        {
            var _con = new AbilityPoisonDamage(MnemonicCode.Con, new DieRoller(3));
            var _dex = new AbilityPoisonDamage(MnemonicCode.Dex, new DieRoller(3));
            var _rise = new GhoulRiserDiseaseDamage();
            return new Disease(@"Ghoul Fever", new PoisonMultiDamage(_con, _dex, _rise),
                new ConstantRoller(1), new Day(), 2,
                Creature.GetIntrinsicPowerDifficulty(Creature.Classes.Get<UndeadClass<Ghoul>>(), MnemonicCode.Cha, typeof(DiseaseTrait)),
                Disease.InfectionVector.Injury);
        }
        #endregion

        #region IPoisonProvider
        public Poison GetPoison()
        {
            var _sickened = new SickenedPoisonDamage(new ComplexDiceRoller(@"1d6+4"), new Minute());
            var _none = new NoPoisonDamage();

            // build poison
            var _class = Creature.Classes.Get<UndeadClass<Ghoul>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Cha, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Ghastly Stench", _sickened, _none,
                Poison.ActivationMethod.Inhaled, Poison.MaterialForm.Liquid, _difficulty,
                Creature.ID, Hour.UnitFactor * 24);
        }
        #endregion

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
                var _stenchSize = new GeometricSize(
                    _critterSize.ZHeight + 4,
                    _critterSize.YLength + 4,
                    _critterSize.XLength + 4);
                var _reachZone = new CubicBuilder(_stenchSize, _stenchSize.CenterCell(_expectEven));
                // TODO: should have a spread instead of a cubic for the capture zone

                // cubic capture
                return new CubicBuilder(_stenchSize, _stenchSize.CenterCell(_expectEven));
            }
            return new CubicBuilder(new GeometricSize(4,4,4));
        }

        #region IReplaceCreature Members

        public bool CanGenerate
        {
            get
            {
                return (Original.CreatureType is HumanoidType)
                    && _Original.CreatureType.IsLiving;
            }
        }

        #endregion
    }

    [Serializable]
    public class GhoulRiserDiseaseDamage : PoisonDamage
    {
        public override string Name => @"Rise ghoulishly if killed by disease";

        #region private void TryStartReAnimation(Creature critter)
        private void TryStartReAnimation(Creature critter)
        {
            // just died?
            var _now = critter?.GetCurrentTime() ?? 0d;
            var _dead = critter?.Adjuncts.OfType<DeadEffect>().Where(_de => _de.IsActive).FirstOrDefault();

            // died in same tick that damage was taken, good enough...
            if (_dead?.TimeOfDeath == _now)
            {
                // timer to rise (random number of hours 3d4...)
                var _riseTime = _now + DiceRoller.RollDice(critter.ID, 3, 4, @"Reanimate Time", @"Hours") * Hour.UnitFactor;
                critter.AddAdjunct(new PendingGhoul(this, _riseTime));
            }
        }
        #endregion

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            TryStartReAnimation(critter);
            yield break;
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            TryStartReAnimation(critter);
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield break;
        }
    }

    [Serializable]
    public class PendingGhoul : Adjunct, ITrackTime
    {
        public PendingGhoul(GhoulRiserDiseaseDamage source, double time)
            : base(source)
        {
            _Time = time;
        }

        #region data
        private double _Time;
        #endregion

        public GhoulRiserDiseaseDamage GhoulRiserDiseaseDamage => Source as GhoulRiserDiseaseDamage;
        public double Time => _Time;
        public double Resolution => Hour.UnitFactor;

        public override object Clone()
            => new PendingGhoul(GhoulRiserDiseaseDamage, Time);

        #region public void TrackTime(double timeVal, TimeValTransition direction)
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= Time)
            {
                // if time expired and still dead, rise as ghoul/ghast
                if ((Anchor as Creature)?.HasActiveAdjunct<DeadEffect>() ?? false)
                {
                    // make a new ghoul
                    var _critter = Anchor as Creature;
                    var _ghoul = new Ghoul(_critter);
                    var _replace = new Creature(@"Ghoul", _ghoul.DefaultAbilities());
                    _ghoul.BindTo(_replace);
                    _replace.Devotion = new Devotion(@"Death Magic");

                    // place ghoul in environment, remove old creature...
                    if (Anchor?.GetLocated()?.Locator is ObjectPresenter _presenter)
                    {
                        var _newPresenter = new ObjectPresenter(_replace, _presenter.MapContext, _presenter.ModelKey,
                            _presenter.NormalSize, _presenter.GeometricRegion);
                        _presenter.MapContext.Remove(_presenter);
                    }
                }

                // eject when time elapsed regardless of rising condition
                Eject();
            }
        }
        #endregion
    }
}
