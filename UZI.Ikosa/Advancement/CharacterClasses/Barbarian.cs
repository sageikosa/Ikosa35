using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.TypeListers;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Barbarian", 12, 1d, 4, true, false, false)]
    [Serializable]
    public class Barbarian : CharacterClass, IWeaponProficiency, IArmorProficiency, IShieldProficiency,
        IActionProvider, IPowerClass, IDamageReduction, IActionSource
    {
        public Barbarian()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Barbarian(PowerDieCalcMethod initMethod)
            : base(12, initMethod)
        {
            // TODO:
            _PowerLevel = new DeltableQualifiedDelta(0, @"Class Power Level", this);
            _PowerLevel.Deltas.Add(_LevelDelta);
            _RageBattery = new RegeneratingBattery(this, 1, Day.UnitFactor);
        }

        #region data
        private DeltableQualifiedDelta _PowerLevel;
        // TODO: illiteracy
        private RegeneratingBattery _RageBattery;
        #endregion

        // IActionProvider Interface
        #region public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: consider this as an IActionProvider adjunct...
            if (Creature.Alignment.Orderliness != LawChaosAxis.Lawful)
            {
                if (!Creature.HasActiveAdjunct<Raging>())
                {
                    // TODO: other conditions that might prevent raging?
                    if (!Creature.Conditions.Contains(Condition.Fatigued)
                        && !Creature.Conditions.Contains(Condition.Exhausted))
                    {
                        // not raging...can we start a rage?
                        if (_RageBattery.CanUseCharges(1))
                        {
                            var _level = ClassPowerLevel.EffectiveValue;
                            if (_level < 11) // 1..10
                            {
                                // regular rage (10 minute fatigue)
                                yield return new StartRage(this, 4, 2, 100, @"101");
                            }
                            else if (_level < 14) // 11..13
                            {
                                // greater rage (5 minute fatigue)
                                yield return new StartRage(this, 6, 3, 50, @"101");
                            }
                            else if (_level < 17) // 14..16
                            {
                                // greater rage + indomitable will (5 minute fatigue)
                                yield return new StartRage(this, 6, 3, 50, @"101", new IndomitableWill(this));
                            }
                            else if (_level < 20) // 17..19
                            {
                                // greater + indomitable will + tireless rage (no fatigue)
                                yield return new StartRage(this, 6, 3, 0, @"101", new IndomitableWill(this));
                            }
                            else // >=20
                            {
                                // mighty rage + indomitable will (no fatigue)
                                yield return new StartRage(this, 8, 4, 0, @"101", new IndomitableWill(this));
                            }
                        }
                    }
                }
                else
                {
                    yield return new EndRage(this, @"101");
                }
            }
            yield break;
        }
        #endregion

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToPowerClassInfo();

        // other stuff
        public override string ClassName => @"Barbarian";
        public override int SkillPointsPerLevel => 4;
        public override double BABProgression => 1;
        public override bool HasGoodFortitude => true;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => false;

        #region IPowerClass Members
        public IVolatileValue ClassPowerLevel => _PowerLevel;
        public bool IsPowerClassActive { get => true; set { } }
        public string ClassIconKey => @"barbarian_class";

        public Guid OwnerID
            => (Creature?.ID ?? Guid.Empty);

        public PowerClassInfo ToPowerClassInfo()
            => new PowerClassInfo
            {
                OwnerID = OwnerID.ToString(),
                Key = Key,
                ID = ID,
                Message = ClassName,
                IsPowerClassActive = IsPowerClassActive,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #region IControlChange<Activation> Members

        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        #endregion

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _PowerLevel.QualifiedDeltas(qualify);

        private TerminateController _TCtrl;
        private TerminateController Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => Term.TerminateSubscriberCount;

        #endregion

        // IActionSource Members
        public IVolatileValue ActionClassLevel => _PowerLevel;

        // Character Class
        #region protected override void OnIncreaseLevel()
        protected override void OnIncreaseLevel()
        {
            switch (CurrentLevel)
            {
                case 1:
                    Creature?.AddAdjunct(new FastMovement(this));
                    // TODO: illiteracy (*)
                    break;

                case 2:
                    // uncanny dodge if not already present, otherwise improved uncanny dodge
                    if (!(Creature?.HasAdjunct<UncannyDodge>() ?? false))
                        Creature?.AddAdjunct(new UncannyDodge(this));
                    else
                        Creature?.AddAdjunct(new ImprovedUncannyDodge(this));
                    Creature?.AddAdjunct(new ImprovedUncannyDodgeSupplier(this));
                    break;

                case 3:
                    Creature?.AddAdjunct(new TrapSenseSave(this));
                    Creature?.AddAdjunct(new TrapSenseDodge(this));
                    break;

                case 4:
                    _RageBattery.MaximumCharges.BaseValue += 1;
                    break;

                case 5:
                    if (!(Creature?.HasAdjunct<ImprovedUncannyDodge>() ?? false))
                        Creature?.AddAdjunct(new ImprovedUncannyDodge(this));
                    break;

                case 7:
                    Creature?.DamageReductions.Add(this);
                    break;

                case 8:
                    _RageBattery.MaximumCharges.BaseValue += 1;
                    break;

                case 12:
                    _RageBattery.MaximumCharges.BaseValue += 1;
                    break;

                case 16:
                    _RageBattery.MaximumCharges.BaseValue += 1;
                    break;

                case 20:
                    _RageBattery.MaximumCharges.BaseValue += 1;
                    break;
            }
            base.OnIncreaseLevel();
        }
        #endregion

        #region protected override void OnDecreaseLevel()
        protected override void OnDecreaseLevel()
        {
            switch (CurrentLevel)
            {
                case 0:
                    Creature?.Adjuncts.OfType<FastMovement>().FirstOrDefault(_fm => _fm.PowerClass == this)?.Eject();
                    // TODO: remove illiteracy (*)
                    break;

                case 1:
                    // uncanny dodge, or improved uncanny dodge, just in case
                    Creature?.Adjuncts.OfType<UncannyDodge>().FirstOrDefault(_ud => _ud.Source == this)?.Eject();
                    Creature?.Adjuncts.OfType<ImprovedUncannyDodge>().FirstOrDefault(_iud => _iud.Source == this)?.Eject();
                    Creature?.Adjuncts.OfType<ImprovedUncannyDodgeSupplier>().FirstOrDefault(_iuds => _iuds.Source == this)?.Eject();
                    break;

                case 2:
                    Creature?.Adjuncts.OfType<TrapSenseSave>().FirstOrDefault(_ts => _ts.PowerClass == this)?.Eject();
                    Creature?.Adjuncts.OfType<TrapSenseDodge>().FirstOrDefault(_td => _td.PowerClass == this)?.Eject();
                    break;

                case 3:
                    _RageBattery.MaximumCharges.BaseValue -= 1;
                    break;

                case 4:
                    // never got uncanny dodge for this class? then improved removes at level 2 to 1 removal
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        Creature?.Adjuncts.OfType<ImprovedUncannyDodge>()
                            .FirstOrDefault(_iud => _iud.Source == this)?.Eject();
                    }
                    break;

                case 6:
                    Creature?.DamageReductions.Remove(this);
                    break;

                case 7:
                    _RageBattery.MaximumCharges.BaseValue -= 1;
                    break;

                case 11:
                    _RageBattery.MaximumCharges.BaseValue -= 1;
                    break;

                case 15:
                    _RageBattery.MaximumCharges.BaseValue -= 1;
                    break;

                case 19:
                    _RageBattery.MaximumCharges.BaseValue -= 1;
                    break;
            }
            base.OnDecreaseLevel();
        }
        #endregion

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            const string _rage = @"Fly into a rage, with bonuses, penalties and action restrictions.";
            switch (level)
            {
                case 1:
                    yield return new Feature(@"Fast Movement", @"+10 ft to land movement when not greatly encumbered");
                    // TODO: illiteracy
                    yield return new Feature(@"Rage 1/day", _rage);
                    break;
                case 2:
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        yield return new Feature(@"Uncanny Dodge", @"Retain Dexterity to AR if flat-footed or against invisible attacker");
                    }
                    else
                    {
                        yield return new Feature(@"Improved Uncanny Dodge", @"Cannot be flanked except by higher level sneak attacker");
                    }
                    break;
                case 3:
                    yield return new Feature(@"Trap Sense", @"+1 Reflex and +1 Dodge versus traps");
                    break;
                case 4:
                    yield return new Feature(@"Rage 2/day", _rage);
                    break;
                case 5:
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        yield return new Feature(@"Improved Uncanny Dodge", @"Cannot be flanked except by higher level sneak attacker");
                    }
                    break;
                case 6:
                    yield return new Feature(@"Trap Sense", @"+2 Reflex and +2 Dodge versus traps");
                    break;
                case 7:
                    yield return new Feature(@"Damage Reduction 1", @"DR 1/-");
                    break;
                case 8:
                    yield return new Feature(@"Rage 3/day", _rage);
                    break;
                case 9:
                    yield return new Feature(@"Trap Sense", @"+3 Reflex and +3 Dodge versus traps");
                    break;
                case 10:
                    yield return new Feature(@"Damage Reduction 2", @"DR 2/-");
                    break;
                case 11:
                    yield return new Feature(@"Greater Rage", @"Improved bonuses on rage");
                    break;
                case 12:
                    yield return new Feature(@"Rage 4/day", _rage);
                    yield return new Feature(@"Trap Sense", @"+4 Reflex and +4 Dodge versus traps");
                    break;
                case 13:
                    yield return new Feature(@"Damage Reduction 3", @"DR 3/-");
                    break;
                case 14:
                    yield return new Feature(@"Indomitable Will", @"+4 Will versus Enchantment while raging");
                    break;
                case 15:
                    yield return new Feature(@"Trap Sense", @"+5 Reflex and +5 Dodge versus traps");
                    break;
                case 16:
                    yield return new Feature(@"Rage 5/day", _rage);
                    yield return new Feature(@"Damage Reduction 4", @"DR 4/-");
                    break;
                case 17:
                    yield return new Feature(@"Tireless Rage", @"No post-rage fatigue");
                    break;
                case 18:
                    yield return new Feature(@"Trap Sense", @"+6 Reflex and +6 Dodge versus traps");
                    break;
                case 19:
                    yield return new Feature(@"Damage Reduction 5", @"DR 5/-");
                    break;
                case 20:
                    yield return new Feature(@"Mighty Rage", @"Improved bonuses on rage");
                    yield return new Feature(@"Rage 6/day", _rage);
                    break;
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(HandleAnimalSkill);
            yield return typeof(IntimidateSkill);
            yield return typeof(JumpSkill);
            yield return typeof(ListenSkill);
            yield return typeof(RideSkill);
            yield return typeof(SurvivalSkill);
            yield return typeof(SwimSkill);
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => ((profType == WeaponProficiencyType.Simple) || (profType == WeaponProficiencyType.Martial))
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // everything but exotic weapons (generally)
            return (!typeof(IExoticWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));
        }

        string IWeaponProficiency.Description { get { return @"All simple and martial weapons"; } }

        #endregion

        #region IArmorProficiency Members

        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
        {
            // proficient with light armor and medium armors
            return (profType < ArmorProficiencyType.Heavy)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            // proficient with light armor and medium armors
            return (armor.ProficiencyType > ArmorProficiencyType.Heavy)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        string IArmorProficiency.Description
            => @"Light and Medium Armor";

        #endregion

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
            => !tower && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => IsProficientWithShield(shield.Tower, powerLevel);

        string IShieldProficiency.Description => @"Normal Shields";
        #endregion

        #region IDamageReduction Members
        public bool WeaponIgnoresReduction(IWeaponHead weaponHead)
            => false;

        public int Amount
            => (CurrentLevel < 7) ? 0
            : (CurrentLevel < 10) ? 1
            : (CurrentLevel < 13) ? 2
            : (CurrentLevel < 16) ? 3
            : (CurrentLevel < 19) ? 4
            : 5;

        public string Name
            => $@"DR {Amount}/-";

        public object Source
            => this;

        public void HasReduced(int amount) { }
        #endregion
    }

    [Serializable]
    public class FastMovement : Adjunct, IMonitorChange<EncumberanceVal>
    {
        public FastMovement(IPowerClass source)
            : base(source)
        {
            _Delta = new Delta(10, typeof(FastMovement), @"Fast Movement");
        }

        #region data
        private Delta _Delta;
        #endregion

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public Delta Delta => _Delta;

        private Creature Critter
            => Anchor as Creature;

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // track encumberance
            Critter?.EncumberanceCheck.AddChangeMonitor(this);

            // bootstrap?
            if ((Critter?.EncumberanceCheck.EncumberanceValue ?? EncumberanceVal.Overloaded) <= EncumberanceVal.Encumbered)
            {
                // each land movement gets a boost
                foreach (var _land in Critter.Movements.AllMovements.OfType<LandMovement>())
                    _land.Deltas.Add(_Delta);
            }
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            // terminate boost
            _Delta.DoTerminate();

            // stop tracking encumberance
            Critter?.EncumberanceCheck.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new FastMovement(PowerClass);

        // IMonitorChange<EncumberanceVal>
        public void PreTestChange(object sender, AbortableChangeEventArgs<EncumberanceVal> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<EncumberanceVal> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<EncumberanceVal> args)
        {
            if ((args.OldValue >= EncumberanceVal.GreatlyEncumbered)
                && (args.NewValue <= EncumberanceVal.Encumbered))
            {
                // switching on
                foreach (var _land in Critter.Movements.AllMovements.OfType<LandMovement>())
                    _land.Deltas.Add(_Delta);
            }
            else if ((args.OldValue <= EncumberanceVal.Encumbered)
                && (args.NewValue >= EncumberanceVal.GreatlyEncumbered))
            {
                // switching off
                _Delta.DoTerminate();
            }
        }
    }

    [Serializable]
    public class IndomitableWill : Adjunct, IQualifyDelta
    {
        public IndomitableWill(IPowerClass source)
            : base(source)
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(4, typeof(IndomitableWill), @"Indomitable Will");
        }

        #region data
        private IDelta _Delta;
        private TerminateController _Terminator;
        #endregion

        private Creature Critter
            => Anchor as Creature;

        public IDelta Delta => _Delta;

        public IPowerClass PowerClass
            => Source as IPowerClass;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Critter?.WillSave.Deltas.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new IndomitableWill(PowerClass);

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => ((qualify?.Source as SpellSource)?.MagicStyle is Enchantment
            ? _Delta
            : null).ToEnumerable().Where(_d => _d != null);

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
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
}
