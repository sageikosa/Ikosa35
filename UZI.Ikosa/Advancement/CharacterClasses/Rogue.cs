using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Contracts;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Rogue", 6, 0.75d, 8, false, true, false)]
    [Serializable]
    public class Rogue : CharacterClass, IWeaponProficiency, IArmorProficiency, IPowerClass
    {
        public Rogue()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Rogue(PowerDieCalcMethod initMethod)
            : base(6, initMethod)
        {
            _PowerLevel = new DeltableQualifiedDelta(0, @"Class Power Level", this);
            _PowerLevel.Deltas.Add(_LevelDelta);
            // TODO: track selectable features above 10th level
        }

        #region data
        private DeltableQualifiedDelta _PowerLevel;
        #endregion

        public override string ClassName => @"Rogue";
        public override int SkillPointsPerLevel => 8;
        public override double BABProgression => 0.75;
        public override bool HasGoodFortitude => false;
        public override bool HasGoodReflex => true;
        public override bool HasGoodWill => false;

        #region IPowerClass Members
        public IVolatileValue ClassPowerLevel => _PowerLevel;
        public bool IsPowerClassActive { get => true; set { } }
        public string ClassIconKey => @"rogue_class";

        public Guid OwnerID
            => (Creature?.ID ?? Guid.Empty);

        public PowerClassInfo ToPowerClassInfo()
        {
            return new PowerClassInfo
            {
                OwnerID = OwnerID.ToString(),
                Key = Key,
                ID = ID,
                Message = ClassName,
                IsPowerClassActive = IsPowerClassActive,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };
        }

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
        private TerminateController _Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        protected override void OnAdd()
        {
            base.OnAdd();
        }

        protected override void OnRemove()
        {
            base.OnRemove();
        }

        #region protected override void OnIncreaseLevel()
        protected override void OnIncreaseLevel()
        {
            switch (CurrentLevel)
            {
                case 1:
                    Creature?.AddAdjunct(new SneakAttack(this, 1, 2));
                    Creature?.AddAdjunct(new TrapFinding(this));
                    break;
                case 2:
                    Creature?.AddAdjunct(new Interactor<EvasionHandler>(this));
                    break;
                case 3:
                    Creature?.AddAdjunct(new TrapSenseSave(this));
                    Creature?.AddAdjunct(new TrapSenseDodge(this));
                    break;
                case 4:
                    // uncanny dodge if not already present, otherwise improved uncanny dodge
                    if (!(Creature?.HasAdjunct<UncannyDodge>() ?? false))
                        Creature?.AddAdjunct(new UncannyDodge(this));
                    else
                        Creature?.AddAdjunct(new ImprovedUncannyDodge(this));
                    Creature?.AddAdjunct(new ImprovedUncannyDodgeSupplier(this));
                    break;
                case 8:
                    if (!(Creature?.HasAdjunct<ImprovedUncannyDodge>() ?? false))
                        Creature?.AddAdjunct(new ImprovedUncannyDodge(this));
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
                    Creature?.Adjuncts.OfType<SneakAttack>().FirstOrDefault(_i => _i.PowerClass == this)?.Eject();
                    Creature?.Adjuncts.OfType<TrapFinding>().FirstOrDefault(_i => _i.PowerClass == this)?.Eject();
                    break;
                case 1:
                    Creature?.Adjuncts.OfType<Interactor<EvasionHandler>>().FirstOrDefault(_i => _i.Source == this)?.Eject();
                    break;
                case 3:
                    // uncanny dodge, or improved uncanny dodge, just in case
                    Creature?.Adjuncts.OfType<UncannyDodge>().FirstOrDefault(_ud => _ud.Source == this)?.Eject();
                    Creature?.Adjuncts.OfType<ImprovedUncannyDodge>().FirstOrDefault(_iud => _iud.Source == this)?.Eject();
                    Creature?.Adjuncts.OfType<ImprovedUncannyDodgeSupplier>().FirstOrDefault(_iuds => _iuds.Source == this)?.Eject();
                    break;
                case 2:
                    Creature?.Adjuncts.OfType<TrapSenseSave>().FirstOrDefault(_ts => _ts.PowerClass == this)?.Eject();
                    Creature?.Adjuncts.OfType<TrapSenseDodge>().FirstOrDefault(_td => _td.PowerClass == this)?.Eject();
                    break;
                case 7:
                    // never got uncanny dodge for this class? then improved removes at level 4 to 3 removal
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        Creature?.Adjuncts.OfType<ImprovedUncannyDodge>()
                            .FirstOrDefault(_iud => _iud.Source == this)?.Eject();
                    }
                    break;
            }
            base.OnDecreaseLevel();
        }
        #endregion

        public override IEnumerable<IFeature> Features(int level)
        {
            // TODO: selectable features...
            const string _sneak = @"Strike vulnerable opponent for extra damage";
            switch (level)
            {
                case 1:
                    yield return new Feature(@"Sneak Attack +1d6", _sneak);
                    yield return new Feature(@"Trap Finding", @"Locate and disable difficult traps");
                    break;
                case 2:
                    yield return new Feature(@"Evasion", @"No damage if succeeds on a reflex save for half damage.");
                    break;
                case 3:
                    yield return new Feature(@"Sneak Attack +2d6", _sneak);
                    yield return new Feature(@"Trap Sense", @"+1 Reflex and +1 Dodge versus traps");
                    break;
                case 4:
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        yield return new Feature(@"Uncanny Dodge", @"Retain Dexterity to AR if flat-footed or against invisible attacker");
                    }
                    else
                    {
                        yield return new Feature(@"Improved Uncanny Dodge", @"Cannot be flanked except by higher level sneak attacker");
                    }
                    break;
                case 5:
                    yield return new Feature(@"Sneak Attack +3d6", _sneak);
                    break;
                case 6:
                    yield return new Feature(@"Trap Sense", @"+2 Reflex and +2 Dodge versus traps");
                    break;
                case 7:
                    yield return new Feature(@"Sneak Attack +4d6", _sneak);
                    break;
                case 8:
                    if (Creature?.Adjuncts.OfType<UncannyDodge>().Any(_ud => _ud.PowerClass == this) ?? false)
                    {
                        yield return new Feature(@"Improved Uncanny Dodge", @"Cannot be flanked except by higher level sneak attacker");
                    }
                    break;
                case 9:
                    yield return new Feature(@"Sneak Attack +5d6", _sneak);
                    yield return new Feature(@"Trap Sense", @"+3 Reflex and +3 Dodge versus traps");
                    break;
                case 11:
                    yield return new Feature(@"Sneak Attack +6d6", _sneak);
                    break;
                case 12:
                    yield return new Feature(@"Trap Sense", @"+4 Reflex and +4 Dodge versus traps");
                    break;
                case 13:
                    yield return new Feature(@"Sneak Attack +7d6", _sneak);
                    break;
                case 15:
                    yield return new Feature(@"Sneak Attack +8d6", _sneak);
                    yield return new Feature(@"Trap Sense", @"+5 Reflex and +5 Dodge versus traps");
                    break;
                case 17:
                    yield return new Feature(@"Sneak Attack +9d6", _sneak);
                    break;
                case 18:
                    yield return new Feature(@"Trap Sense", @"+6 Reflex and +6 Dodge versus traps");
                    break;
                case 19:
                    yield return new Feature(@"Sneak Attack +10d6", _sneak);
                    break;
            }
            // TODO: crippling strike, defensive roll, improved evasion, 
            //       opportunist, skill mastery, slippery mind, bonus feat
            yield break;
        }

        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            // TODO: choose from crippling strike, defensive roll, improved evasion, 
            //       opportunist, skill mastery, slippery mind, bonus feat
            return base.Requirements(level);
        }

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(AppraiseSkill);
            yield return typeof(BalanceSkill);
            yield return typeof(BluffSkill);
            yield return typeof(ClimbSkill);
            yield return typeof(DecipherScriptSkill);
            yield return typeof(DiplomacySkill);
            yield return typeof(DisableMechanismSkill);
            yield return typeof(DisguiseSkill);
            yield return typeof(EscapeArtistSkill);
            yield return typeof(ForgerySkill);
            yield return typeof(GatherInformationSkill);
            yield return typeof(StealthSkill);
            yield return typeof(IntimidateSkill);
            yield return typeof(JumpSkill);
            yield return typeof(ListenSkill);
            yield return typeof(SilentStealthSkill);
            yield return typeof(PickLockSkill);
            yield return typeof(SearchSkill);
            yield return typeof(SenseMotiveSkill);
            yield return typeof(QuickFingersSkill);
            yield return typeof(SpotSkill);
            yield return typeof(SwimSkill);
            yield return typeof(TumbleSkill);
            yield return typeof(UseMagicItemSkill);
            yield return typeof(UseRopeSkill);
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<KnowledgeFocus>(typeof(KnowledgeSkill<>)))
            {
                yield return _skillType;
            }
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<PerformFocus>(typeof(PerformSkill<>)))
            {
                yield return _skillType;
            }
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<ProfessionFocus>(typeof(ProfessionSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType == WeaponProficiencyType.Simple)
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        #region public bool IsProficientWithWeapon(Type type, int powerLevel)
        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // certain martial/exotic weapons
            if (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level)
            {
                if (type == typeof(HandCrossbow))
                    return true;
                if (type == typeof(Rapier))
                    return true;
                if (type == typeof(Sap))
                    return true;
                if (type == typeof(ShortSword))
                    return true;
                if (type == typeof(ShortBow))
                    return true;

                // simple weapons
                return (!typeof(IMartialWeapon).IsAssignableFrom(type)
                    && !typeof(IExoticWeapon).IsAssignableFrom(type));
            }
            return false;
        }
        #endregion

        string IWeaponProficiency.Description
            => @"All simple weapons, plus hand crossbow, rapier, sap, shortbow and short sword";

        #endregion

        #region IArmorProficiency Members

        // proficient with light armor
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
            => (profType <= ArmorProficiencyType.Light) 
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        // proficient with light armor
        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => (armor.ProficiencyType <= ArmorProficiencyType.Light) 
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        string IArmorProficiency.Description
            => @"Light Armor";

        #endregion
    }
}
