using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Universal;
using Uzi.Packaging;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Packaging;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    // TODO: sort out the events
    [Serializable]
    public class Creature : CoreActor, IVisible, ISizable, ICorePart, IMonitorChange<Physical>, ISensorHost, IActionSource,
        IProvideSaves, IDeserializationCallback
    {
        #region ctor
        public Creature(string name, AbilitySet abilities)
            : base(name)
        {
            // override possessionSet with cost calculating variety
            _Possess = new IkosaPossessions(this);

            // need this before body
            _Profs = new ProficiencySet(this);

            // body
            BodyDock = new BodyDock(this);
            Body _body = new NoBody(Size.Medium, 1);
            _body.BindTo(this);

            // object load
            ObjectLoad.AddChangeMonitor(this);

            SpeciesDock = new SpeciesDock(this);
            CreatureTypeDock = new CreatureTypeDock(this);
            SubTypes = new WatchableSet<CreatureSubType>();
            _ExtraSpellDifficulty = new DeltableQualifiedDelta(0, @"Spell Difficulty Qualifiers", this);
            _ExtraAbilityCheck = new DeltableQualifiedDelta(0, @"Ability Check Qualifiers", this);
            _ExtraSkillCheck = new DeltableQualifiedDelta(0, @"Skill Check Qualifiers", this);
            _ExtraCriticalRange = new DeltableQualifiedDelta(0, @"Improved Critical", typeof(WeaponHead));
            _ExtraCriticalDamage = new DeltableQualifiedDelta(0, @"Augmented Critical", typeof(WeaponHead));
            _ExtraDrivingBattery = new DeltableQualifiedDelta(0, @"Extra Driving Attempts", typeof(DriveCreaturePowerDef));
            _ExtraClassPowerLevel = new DeltableQualifiedDelta(0, @"Extra Class Power Level", typeof(IPowerClass));
            _MultiWpnDelta = new MultiWeaponDelta();

            // BAB
            var _bab = BaseAttack;
            MeleeDeltable.Deltas.Add(_bab);
            _Ranged.Deltas.Add(_bab);
            _Opposed.Deltas.Add(_bab);

            // Abilities, init, size
            _Abilities = abilities;
            foreach (var _ability in Abilities.AllAbilities)
            {
                // chain extra ability checks
                _ability.CheckQualifiers.Deltas.Add(ExtraAbilityCheck);
            }
            _AbilityMonitor = new AbilityMonitor(this);
            _Init = new ConstDeltable(0);
            _SResist = new ConstDeltable(0);

            // Encumberance
            _RunFactor = new ConstDeltable(4);  // must be set before encumberance
            _MaxDexterityToARBonus = new MaxDexterityToARBonus(this);
            CarryingCapacity = new CarryingCapacity(this);
            _EncumberanceCheck = new Encumberance(this);

            // sets of things
            _Skills = new Uzi.Ikosa.Skills.SkillSet(this);
            _Feats = new Uzi.Ikosa.Feats.FeatSet(this);
            HeldItemsGroups = new HeldItemsGroupSet(this);
            _Languages = new LanguageSet();
            _Senses = new SensorySet();
            _Senses.BindTo(this);
            _Moves = new MovementSet(this);
            _MoveDoublings = new ConstDeltable(0);
            _DivMarks = new ExtraInfoSet(this);
            _Classes = new AdvancementClassSet(this);
            _AdvLog = new AdvancementLog(this);
            _TempHP = new TempHPSet(this);
            _HP = new HealthPoints(this);
            _Conds = new ConditionSet(this);
            _DmgReduces = new DamageReductionSet(this);
            _EnergyResist = new EnergyResistanceSet(this);
            Awarenesses = new AwarenessSet(this);
            _Rooms = new RoomAwarenessSet(this);
            _Sounds = new SoundAwarenessSet(this);
            _FriendlyCreatures = new Collection<Guid>();
            _UnfriendlyCreatures = new Collection<Guid>();
            _InfoKeys = new Collection<Guid>();
            _Age = 0;
            _Gender = Gender.Genderless;

            // Create AR values 
            _NormalAR = new Deltable(10);
            _TouchAR = new Deltable(10);
            _IncorporealAR = new Deltable(10);

            // hook ability modifiers
            HookAbilityModifiers();

            // Hook body modifiers
            HookBodyModifiers();

            // hook proficiencies
            HookCombatDeltas();

            InitializeInteractionHandlers();
        }

        #region Setup: HookAbilityModifiers(), HookBodyModifiers(), and HookCombatDeltas()
        private void HookAbilityModifiers()
        {
            // MaxDex
            _NormalAR.Deltas.Add(_MaxDexterityToARBonus);
            _TouchAR.Deltas.Add(_MaxDexterityToARBonus);
            _IncorporealAR.Deltas.Add(_MaxDexterityToARBonus);

            // Dexterity Modifiers
            var _dexMod = Abilities.Dexterity;
            _ReflexSave.Deltas.Add(_dexMod);
            _Ranged.Deltas.Add(_dexMod);
            Initiative.Deltas.Add(_dexMod);

            // Constitution
            var _conMod = Abilities.Constitution;
            _FortitudeSave.Deltas.Add(_conMod);

            // WillSave
            var _wisMod = Abilities.Wisdom;
            _WillSave.Deltas.Add(_wisMod);

            // Intelligence

            // Strength
            var _strMod = Abilities.Strength;
            _Melee.Deltas.Add(_strMod);
            _Opposed.Deltas.Add(_strMod);

            // qualified deltas based on strength...
            _ExtraWeaponDamage.Deltas.Add(new MeleeStrengthDamage(this));
            _ExtraWeaponDamage.Deltas.Add(new MeleeHalfStrengthDamage(this));
            _ExtraWeaponDamage.Deltas.Add(new MeleeOverStrengthDamage(this));
            _ExtraWeaponDamage.Deltas.Add(new ProjectileStrengthDamage(this));

            // Charisma
        }

        private void HookBodyModifiers()
        {
            // Size Modifiers
            _TouchAR.Deltas.Add(BodyDock.SizeModifier);
            _NormalAR.Deltas.Add(BodyDock.SizeModifier);
            _IncorporealAR.Deltas.Add(BodyDock.SizeModifier);
            _Melee.Deltas.Add(BodyDock.SizeModifier);
            _Ranged.Deltas.Add(BodyDock.SizeModifier);
            var _sizeDiff = new WeaponSizePenalty(this);
            _Melee.Deltas.Add(_sizeDiff);
            _Ranged.Deltas.Add(_sizeDiff);

            // Special Size Modifiers
            _Opposed.Deltas.Add(BodyDock.OpposedModifier);

            // Natural Armor
            _NormalAR.Deltas.Add(BodyDock.NaturalArmorModifier);

            // hide modifier
            Skills[typeof(Skills.StealthSkill)].Deltas.Add(BodyDock.HideModifier);
        }

        private void HookCombatDeltas()
        {
            MeleeDeltable.Deltas.Add(Proficiencies.ArmorProficiencyDelta);
            MeleeDeltable.Deltas.Add(Proficiencies.ShieldProficiencyDelta);
            MeleeDeltable.Deltas.Add(Proficiencies.WeaponProficiencyDelta);
            RangedDeltable.Deltas.Add(Proficiencies.ArmorProficiencyDelta);
            RangedDeltable.Deltas.Add(Proficiencies.ShieldProficiencyDelta);
            RangedDeltable.Deltas.Add(Proficiencies.WeaponProficiencyDelta);
            OpposedDeltable.Deltas.Add(Proficiencies.ArmorProficiencyDelta);
            OpposedDeltable.Deltas.Add(Proficiencies.ShieldProficiencyDelta);
            OpposedDeltable.Deltas.Add(Proficiencies.WeaponProficiencyDelta);
            OpposedDeltable.Deltas.Add(new OpposedWeaponDelta(this));
        }
        #endregion

        #region Interact Handlers
        protected void InitializeInteractionHandlers()
        {
            AddIInteractHandler(new CreatureAttackHandler());
            AddIInteractHandler(new ConditionAttackHandler());
            AddIInteractHandler(new TransitAttackHandler());
            AddIInteractHandler(new SpellTransitHandler());
            AddIInteractHandler(new FlankingCheckHandler());

            AddIInteractHandler(new SaveFromDamageHandler());
            AddIInteractHandler(new DamageReductionHandler());
            AddIInteractHandler(new EnergyResistanceHandler());
            AddIInteractHandler(new TempHPDamageHandler());

            AddIInteractHandler(new SoundHandler());
            // TODO: project aura handler?
        }
        #endregion
        #endregion

        #region state
        protected ConstDeltable _ExtraWeaponDamage = new ConstDeltable(0);
        protected ConstDeltable _Opportunities = new ConstDeltable(1);
        private bool _NoFriendOpportunity = false;  // NOTE: badly named...with a negative
        protected DeltableQualifiedDelta _ExtraSpellDifficulty;
        protected DeltableQualifiedDelta _ExtraCriticalRange;
        protected DeltableQualifiedDelta _ExtraCriticalDamage;
        protected DeltableQualifiedDelta _ExtraSkillCheck;
        protected DeltableQualifiedDelta _ExtraAbilityCheck;
        protected DeltableQualifiedDelta _ExtraDrivingBattery;
        protected DeltableQualifiedDelta _ExtraClassPowerLevel;
        private Collection<Guid> _FriendlyCreatures;
        private Collection<Guid> _UnfriendlyCreatures;
        private bool _UnknownAsUnFriendly = true;
        private Collection<Guid> _InfoKeys;
        private AbilitySet _Abilities;
        private AbilityMonitor _AbilityMonitor;
        private SkillSet _Skills;
        private MovementSet _Moves;
        private ConstDeltable _RunFactor;
        private ConstDeltable _MoveDoublings = new ConstDeltable(0);
        private SensorySet _Senses;
        private LanguageSet _Languages;
        private Feats.FeatSet _Feats;
        private ExtraInfoSet _DivMarks;
        private RoomAwarenessSet _Rooms;
        private SoundAwarenessSet _Sounds;
        private ProficiencySet _Profs;
        private AdvancementClassSet _Classes;
        private AdvancementLog _AdvLog;
        private TempHPSet _TempHP;
        private HealthPoints _HP;
        private ConditionSet _Conds;
        private ConstDeltable _SResist;
        private ConstDeltable _Init;
        private DamageReductionSet _DmgReduces;
        private EnergyResistanceSet _EnergyResist;
        private double _Age;    // in campaign time-scale
        private Gender _Gender;
        [NonSerialized, JsonIgnore]
        private bool _EditMode = false;
        private double _AimRelLong = 0d;
        private double _AimLat = 0d;
        private double _AimDist = 0d;
        private Point3D _AimPt = new Point3D();
        private int _ThirdRelHeading = 4;
        private int _ThirdIncline = 1;
        private Point3D _ThirdPt = new Point3D();
        private int _Heading = 1;
        private int _Incline = 0;
        // armor rating
        protected Deltable _NormalAR;
        protected Deltable _TouchAR;
        protected Deltable _IncorporealAR;
        protected ConstDeltable _ArcaneFail = new ConstDeltable(0);
        // saves
        protected ConstDeltable _FortitudeSave = new ConstDeltable(0);
        protected ConstDeltable _ReflexSave = new ConstDeltable(0);
        protected ConstDeltable _WillSave = new ConstDeltable(0);
        // encumberance
        protected MaxDexterityToARBonus _MaxDexterityToARBonus;
        protected Encumberance _EncumberanceCheck;
        // attack deltas
        private MultiWeaponDelta _MultiWpnDelta;
        private ConstDeltable _OffHandIterations = new ConstDeltable(1);
        protected BaseAttackValue _BaseAttack = new BaseAttackValue();
        protected Deltable _Melee = new ConstDeltable(0);
        protected Deltable _Ranged = new ConstDeltable(0);
        protected Deltable _Opposed = new ConstDeltable(0);
        #endregion

        protected override ObjectLoad GetInitialObjectLoad()
            => new IkosaObjectLoad(this);

        /// <summary>Indicates the class is being edited by a system editor (as opposed to in-game editing)</summary>
        public bool IsInSystemEditMode { get { return _EditMode; } set { _EditMode = value; } }

        public BodyDock BodyDock { get; private set; }
        public Body Body => BodyDock.Body;

        public SpeciesDock SpeciesDock { get; private set; }
        public Species Species => SpeciesDock.Species;
        public CreatureTypeDock CreatureTypeDock { get; private set; }
        public CreatureType CreatureType => CreatureTypeDock.CreatureType;
        public WatchableSet<CreatureSubType> SubTypes { get; private set; }
        public IEnumerable<TraitBase> Traits
            => Adjuncts.OfType<TraitBase>().Select(_t => _t);

        /// <summary>Declared Allies</summary>
        public Collection<Guid> FriendlyCreatures => _FriendlyCreatures;
        /// <summary>Declared Enemies</summary>
        public Collection<Guid> UnfriendlyCreatures => _UnfriendlyCreatures;
        /// <summary>Any creature not in either FriendlyCreatures or UnfriendlyCreatures is treated as UnFriendly.</summary>
        public bool TreatUnknownAsUnFriendly => _UnknownAsUnFriendly;

        // Armor Rating
        public Deltable NormalArmorRating => _NormalAR;
        public Deltable TouchArmorRating => _TouchAR;
        public Deltable IncorporealArmorRating => _IncorporealAR;
        public ConstDeltable ArcaneSpellFailureChance => _ArcaneFail;

        // Saves
        public ConstDeltable FortitudeSave => _FortitudeSave;
        public ConstDeltable ReflexSave => _ReflexSave;
        public ConstDeltable WillSave => _WillSave;

        // Carrying Capacity
        public MaxDexterityToARBonus MaxDexterityToARBonus => _MaxDexterityToARBonus;
        /// <summary>Creature's encumberance rating, plus the modifier for its check penalty</summary>
        public Encumberance EncumberanceCheck => _EncumberanceCheck;
        public CarryingCapacity CarryingCapacity { get; private set; }

        // Attack Deltas
        public MultiWeaponDelta MultiWeaponDelta => _MultiWpnDelta;
        public ConstDeltable OffHandIterations => _OffHandIterations;
        /// <summary>Do not add qualified deltas</summary>
        public BaseAttackValue BaseAttack => _BaseAttack;
        /// <summary>Melee attack bonus</summary>
        public Deltable MeleeDeltable => _Melee;
        /// <summary>Ranged attack bonus</summary>
        public Deltable RangedDeltable => _Ranged;
        /// <summary>Opposed size-based attack bonus</summary>
        public Deltable OpposedDeltable => _Opposed;

        /// <summary>do not take friendly opportunities if set to true</summary>
        public bool IgnoreFriendlyOpportunities
        {
            get => !_NoFriendOpportunity;
            set => _NoFriendOpportunity = !value;
        }

        /// <summary>Number of opportunistic attacks possible per round</summary>
        public ConstDeltable Opportunities => _Opportunities;
        /// <summary>Strength, Power Attack, Weapon specializations, favored enemies, prayer, etc.</summary>
        public ConstDeltable ExtraWeaponDamage => _ExtraWeaponDamage;
        /// <summary>Weapon qualified improved critical hangs on this</summary>
        public DeltableQualifiedDelta CriticalRangeFactor => _ExtraCriticalRange;
        /// <summary>Qualified improved critical damage hangs on this</summary>
        public DeltableQualifiedDelta CriticalDamageFactor => _ExtraCriticalDamage;
        /// <summary>Spell Focus, etc. not specific to a particular class</summary>
        public DeltableQualifiedDelta ExtraSpellDifficulty => _ExtraSpellDifficulty;
        /// <summary>Deltas that apply to all skill checks</summary>
        public DeltableQualifiedDelta ExtraSkillCheck => _ExtraSkillCheck;
        /// <summary>Deltas that apply to all ability checks</summary>
        public DeltableQualifiedDelta ExtraAbilityCheck => _ExtraAbilityCheck;
        /// <summary>Driving creature attempts not specific to a particular driving power</summary>
        public DeltableQualifiedDelta ExtraDrivingBattery => _ExtraDrivingBattery;

        /// <summary>
        /// Deltas (usually conditional) that may boost power levels for class powers, not specific to a class.
        /// Includes spell penetration and improved creature driving.
        /// </summary>
        public DeltableQualifiedDelta ExtraClassPowerLevel => _ExtraClassPowerLevel;

        public bool IsVisible
            => Invisibility.IsVisible(this);

        /// <summary>Has a 100% critical ignore chance</summary>
        public bool IsImmuneToCriticals
            => CriticalFilterHandler.IsImmuneToCriticals(this);

        public Alignment Alignment
            => this.GetAlignment();

        public AbilitySet Abilities => _Abilities;
        public SkillSet Skills => _Skills;
        public Feats.FeatSet Feats => _Feats;
        public LanguageSet Languages => _Languages;
        public ProficiencySet Proficiencies => _Profs;
        public MovementSet Movements => _Moves;
        public ConstDeltable RunFactor => _RunFactor;
        public double RunCost => 2d / _RunFactor.EffectiveValue;

        /// <summary>Move operates at half speed for every unit (default is zero)</summary>
        public ConstDeltable MoveHalfing
            => _MoveDoublings ??= new ConstDeltable(0);

        /// <summary>Guids of InfoKeys held by the creature</summary>
        public Collection<Guid> InfoKeys => _InfoKeys;

        public AdvancementClassSet Classes => _Classes;
        public AdvancementLog AdvancementLog => _AdvLog;
        public TempHPSet TempHealthPoints => _TempHP;
        public HealthPoints HealthPoints => _HP;
        public ConditionSet Conditions => _Conds;
        public ConstDeltable Initiative => _Init;
        public DamageReductionSet DamageReductions => _DmgReduces;

        /// <summary>All basic "non-stackable" spell-resistance is sourced by typeof(Creature).</summary>
        public ConstDeltable SpellResistance => _SResist;

        /// <summary>All basic "non-stackable" energy-resistance is sourced by typeof(EnergyResistance)</summary>
        public EnergyResistanceSet EnergyResistances => _EnergyResist;

        /// <summary>Equipment kits</summary>
        public HeldItemsGroupSet HeldItemsGroups { get; private set; }

        public IkosaPossessions IkosaPosessions
            => Possessions as IkosaPossessions;

        public string GetMovementSound()
        {
            // TODO: consider makeing this more flexible
            var _armor = Body.ItemSlots.AllSlots.FirstOrDefault(_s => _s.SlotType == ItemSlot.ArmorRobeSlot)?.SlottedItem;
            if (_armor != null)
            {
                return _armor.ItemMaterial.SoundQuality;
            }
            else
            {
                return Body.BodyMaterial.SoundQuality;
            }
        }

        #region public double Age { get; set; }
        public double Age
        {
            get => _Age;
            set
            {
                // NOTE: handle age-related effects as adjuncts on the creature, rather than rule enforced
                _Age = value;
                DoPropertyChanged(nameof(Age));
                DoPropertyChanged(nameof(AgeInYears));
            }
        }
        #endregion

        public double AgeInYears
        {
            get => _Age / Year.UnitFactor;
            set => Age = value * Year.UnitFactor;
        }

        #region Physical Dimensions

        public override double Weight
        {
            get => (Body?.Weight ?? 0) + ObjectLoad.Weight;
            set => DoPropertyChanged(nameof(Weight));
        }

        public override double Height
        {
            get => Body?.Height ?? 0;
            set => DoPropertyChanged(nameof(Height));
        }

        public override double Length
        {
            get => Body?.Length ?? 0;
            set => DoPropertyChanged(nameof(Length));
        }

        public override double Width
        {
            get => Body?.Width ?? 0;
            set => DoPropertyChanged(nameof(Width));
        }

        #endregion

        #region public Gender Gender { get; set; }
        public Gender Gender
        {
            get => _Gender;
            set
            {
                // NOTE: not sure if this needs to signal an in-game change
                _Gender = value;
                DoPropertyChanged(nameof(Gender));
            }
        }
        #endregion

        #region public Devotion Devotion { get; }
        public Devotion Devotion
        {
            get => Adjuncts.OfType<Devotion>().FirstOrDefault();
            set
            {
                if (value != null)
                {
                    AddAdjunct(value);
                    DoPropertyChanged(nameof(Devotion));
                }
            }
        }
        #endregion

        #region Single Creature De-Serialization
        public new static Creature ReadFile(string fileName)
        {
            var _in = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var _fmt = new BinaryFormatter();
            var _creature = _fmt.Deserialize(_in);
            _in.Close();
            return (Creature)_creature;
        }
        #endregion

        #region public override CoreActionBudget CreateActionBudget(CoreTurnTick tick)
        public override CoreActionBudget CreateActionBudget(CoreTurnTick tick)
        {
            if (tick is LocalTurnTick _tick)
            {
                // local action budget
                var _budget = new LocalActionBudget(_tick, this);
                _budget.BudgetItems.Add(typeof(OpportunityBudget), new OpportunityBudget(this, Opportunities));
                return _budget;
            }
            else
            {
                // TODO: non-local budget
                return null;
            }
        }
        #endregion

        public LocalActionBudget GetLocalActionBudget()
            => (ProcessManager as IkosaProcessManager)?.LocalTurnTracker.GetBudget(ID);

        #region public IEnumerable<ActionBase> CombatActionList { get; }
        public IEnumerable<ActionBase> CombatActionList
        {
            get
            {
                // TODO: signal this changing on many other changes...?
                if (Actions != null)
                {
                    foreach (var _act in from _p in Actions.GetActionProviders()
                                         from _a in _p.GetActions(new LocalActionBudget(null, this)).OfType<ActionBase>()
                                         where _a.CombatList
                                         select _a)
                    {
                        yield return _act;
                    }
                }
                yield break;
            }
        }
        #endregion

        public bool IsGrappling(Guid id)
            => Adjuncts.OfType<Grappler>().Any(_g => _g.IsGrappling(id));

        #region public bool CanDodge(Qualifier qualify)
        /// <summary>Returns true if the creature can use dodge bonuses</summary>
        public bool CanDodge(Qualifier qualify)
        {
            // absolute conditions
            if (Conditions.Contains(Condition.Stunned) || Conditions.Contains(Condition.Cowering)
                || Conditions.Contains(Condition.UnpreparedToDodge))
            {
                return false;
            }

            if ((qualify as Interaction)?.InteractData is AttackData _atk)
            {
                // awareness (creature blinded or invisible creature attacker)
                // NOTE: can always use "dodge" to evade a trap (needed for TrapSenseDodge)
                if ((_atk.Attacker != null)
                    && (Awarenesses[_atk.Attacker.ID] < AwarenessLevel.Aware))
                {
                    return false;
                }

                // creature is grappling
                if (Conditions.Contains(Condition.Grappling))
                {
                    // attacker is not grappling the creature
                    if (!IsGrappling(_atk.Attacker.ID))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        /// <summary>Body.Sizer shortcut</summary>
        public Sizer Sizer => Body.Sizer;
        public IGeometricSize GeometricSize => Body.Sizer.Size.CubeSize();

        // ICorePart Members
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public string TypeName => GetType().FullName;

        public override IEnumerable<ICoreObject> Connected
            => (from _is in Body.ItemSlots.AllSlots     // items that are slotted are directly connected
                where (_is.SlottedItem != null)
                && (_is.SlottedItem.BaseObject != null)
                && ((_is.SlottedItem.MainSlot == _is)
                || (_is.SlottedItem.SecondarySlot == _is))
                select _is.SlottedItem.BaseObject).Distinct();

        public override CoreSetting Setting
            => this.GetTokened()?.Token.Context.ContextSet.Setting;

        public double? CurrentTime => (Setting as ITacticalMap)?.CurrentTime;

        #region IMonitorChange<Physical> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
            {
                // notifies others of weight change
                Weight = 0;
            }
            // TODO: other size changes, may need to adjust locator size...alter squeezings...burst confinement
        }

        #endregion

        #region protected override IInteractHandler PostChainHandler(Interaction interact)
        private static CreatureGetInfoDataHandler _CreatureInfoHandler = new CreatureGetInfoDataHandler();
        protected override IInteractHandler PostChainHandler(Interaction interact)
        {
            if (interact != null)
            {
                if (interact.InteractData is GetInfoData)
                {
                    return _CreatureInfoHandler;
                }
            }
            return null;
        }
        #endregion

        protected override string DefaultImage => Species.Name;

        #region public BitmapImagePart GetPortrait()
        public BitmapImagePart GetPortrait(VisualResources resources)
        {
            if (resources != null)
            {
                return (from _key in ImageKeys
                        join _item in resources.ResolvableImages
                        on _key equals _item.BitmapImagePart.Name
                        select _item.BitmapImagePart).FirstOrDefault();
            }
            return null;
        }
        #endregion

        #region private BitmapImageInfo GetPortrait()
        private BitmapImageInfo GetPortraitInfo(VisualResources resources)
        {
            var _image = GetPortrait(resources);
            if (_image != null)
            {
                return new BitmapImageInfo(_image);
            }
            return null;
        }
        #endregion

        // IActionSource Member
        public IVolatileValue ActionClassLevel
            => AdvancementLog.PowerLevel;

        #region ISensorHost Members

        public string SensorHostName => Name;

        public bool IsSensorHostActive => true;

        public SensorySet Senses => _Senses;

        public AwarenessSet Awarenesses { get; private set; }

        /// <summary>Extra information, possibly bound to location or direction from the observer.</summary>
        public ExtraInfoSet ExtraInfoMarkers => _DivMarks;

        public RoomAwarenessSet RoomAwarenesses => _Rooms;
        public SoundAwarenessSet SoundAwarenesses => _Sounds;

        #region public int Heading { get; set; }
        public int Heading
        {
            get { return _Heading; }
            set
            {
                _Heading = value;
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    _AimPt = _loc.Locator.ResyncTacticalPoint(this, AimPointRelativeLongitude, AimPointLatitude, AimPointDistance);
                    _ThirdPt = _loc.Locator.ResyncTacticalPoint(this, ThirdCameraRelativeHeading * 45d, ThirdCameraIncline * 45d, ThirdCameraDistance);
                }
            }
        }
        #endregion

        public int Incline { get => _Incline; set => _Incline = value; }

        #region public double ZOffset { get; }
        public double ZOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperZ - _region.LowerZ + 1);
                    var _alt = _locator.NormalSize.ZExtent;
                    var _off = _locator.IntraModelOffset.Z;
                    var _gravity = _locator.GetGravityFace();
                    var _prone = this.HasActiveAdjunct<ProneEffect>();
                    if (((_gravity == AnchorFace.ZLow) && !_prone)
                        || ((_gravity == AnchorFace.ZHigh) && _prone))
                    {
                        return _alt * 4 + _off;
                    }
                    else if (((_gravity == AnchorFace.ZHigh) && !_prone)
                        || ((_gravity == AnchorFace.ZLow) && _prone))
                    {
                        return _alt * 1 + _off;
                    }
                    else
                    {
                        return _extent * 2.5 + _off;
                    }
                }
                return 2.5d;
            }
        }
        #endregion

        #region public double YOffset { get; }
        public double YOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperY - _region.LowerY + 1);
                    var _alt = _locator.NormalSize.YExtent;
                    var _off = _locator.IntraModelOffset.Y;
                    var _gravity = _locator.GetGravityFace();
                    var _prone = this.HasActiveAdjunct<ProneEffect>();
                    if (((_gravity == AnchorFace.YLow) && !_prone)
                        || ((_gravity == AnchorFace.YHigh) && _prone))
                    {
                        return _alt * 4 + _off;
                    }
                    else if (((_gravity == AnchorFace.YHigh) && !_prone)
                        || ((_gravity == AnchorFace.YLow) && _prone))
                    {
                        return _alt * 1 + _off;
                    }
                    else
                    {
                        return _extent * 2.5 + _off;
                    }
                }
                return 2.5d;
            }
        }
        #endregion

        #region public double XOffset { get; }
        public double XOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperX - _region.LowerX + 1);
                    var _alt = _locator.NormalSize.XExtent;
                    var _off = _locator.IntraModelOffset.X;
                    var _gravity = _locator.GetGravityFace();
                    var _prone = this.HasActiveAdjunct<ProneEffect>();
                    if (((_gravity == AnchorFace.XLow) && !_prone)
                        || ((_gravity == AnchorFace.XHigh) && _prone))
                    {
                        return _alt * 4 + _off;
                    }
                    else if (((_gravity == AnchorFace.XHigh) && !_prone)
                        || ((_gravity == AnchorFace.XLow) && _prone))
                    {
                        return _alt * 1 + _off;
                    }
                    else
                    {
                        return _extent * 2.5 + _off;
                    }
                }
                return 2.5d;
            }
        }
        #endregion

        public double AimPointRelativeLongitude { get => _AimRelLong; set { _AimRelLong = value; } }
        public double AimPointLatitude { get => _AimLat; set { _AimLat = value; } }
        public double AimPointDistance { get => _AimDist; set { _AimDist = value; } }

        public Point3D AimPoint { get { return _AimPt; } set { _AimPt = value; } }

        public int ThirdCameraRelativeHeading { get { return _ThirdRelHeading; } set { _ThirdRelHeading = value; } }
        public int ThirdCameraIncline { get { return _ThirdIncline; } set { _ThirdIncline = value; } }

        #region public double ThirdCameraDistance { get; }
        public double ThirdCameraDistance
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _zExt = _loc.Locator.ZFit;
                    var _yExt = _loc.Locator.YFit;
                    var _xExt = _loc.Locator.XFit;
                    return 0.5d * Math.Sqrt((_zExt * _zExt) + (_yExt * _yExt) + (_xExt * _xExt));
                }
                return AimPointDistance;
            }
        }
        #endregion

        public Point3D ThirdCameraPoint { get { return _ThirdPt; } set { _ThirdPt = value; } }

        #endregion

        #region ITrackTime

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            SoundAwarenesses.Cleanup();
        }

        public double Resolution => Round.UnitFactor;

        #endregion

        #region public IGeometricRegion GetMeleeReachRegion()
        /// <summary>Provides a cubic for the locator region extended by the body's effective reach</summary>
        public IGeometricRegion GetMeleeReachRegion()
        {
            var _loc = this.GetLocated();
            if (_loc != null)
            {
                var _region = _loc.Locator.GeometricRegion;
                var _reach = Body.ReachSquares.EffectiveValue;
                return new Cubic(_region.LowerZ - _reach, _region.LowerY - _reach, _region.LowerX - _reach,
                    _region.UpperZ + _reach, _region.UpperY + _reach, _region.UpperX + _reach);
            }
            return null;
        }
        #endregion

        #region public IEnumerable<Stuff> GetReachable<Stuff>() where Stuff : ICore
        /// <summary>
        /// All accessible stuff in region
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stuff> GetReachable<Stuff>()
            where Stuff : ICore
        {
            var _locator = this.GetLocated()?.Locator;
            var _critterRgn = _locator?.GeometricRegion;
            if (_critterRgn != null)
            {
                // must be in melee reach
                var _reachRgn = GetMeleeReachRegion();
                return from _loc in _locator.MapContext.LocatorsInRegion(_reachRgn, _locator.PlanarPresence)
                       let _obj = _loc.ICoreAs<ICore>().Union(this.ToEnumerable())
                       where _loc.EffectLinesToTarget(_critterRgn, ITacticalInquiryHelper.GetITacticals(_obj.ToArray()).ToArray(),
                        _loc.PlanarPresence).Any()
                       from _stuff in _loc.AllAccessible(this).OfType<Stuff>()
                       where Awarenesses.IsActionAware(_stuff.ID)
                       && ((_stuff as IAdjunctable)?.Adjuncts.OfType<Attended>().FirstOrDefault()?.AllowTacticalActions(this) ?? true)
                       select _stuff;
            }
            return new Stuff[] { };
        }
        #endregion

        #region public CreatureLoginInfo ToCreatureLoginInfo()
        public CreatureLoginInfo ToCreatureLoginInfo()
        {
            var _info = new CreatureLoginInfo()
            {
                ID = ID,
                Name = Name,
                Species = Species.Name,
                Classes = new Collection<ClassInfo>(
                Classes.Select(_ac => _ac.ToClassInfo()).ToList())
            };

            // build class string
            var _builder = new StringBuilder();
            foreach (var _ci in Classes)
            {
                if (_builder.Length > 0)
                    _builder.Append(@"/");
                _builder.AppendFormat(@"{0}-{1}", _ci.ClassName, _ci.CurrentLevel);
            }
            _info.ClassString = _builder.ToString();
            return _info;
        }
        #endregion

        #region public CreatureInfo ToCreatureInfo()
        /// <summary>
        /// Convert to a CreatureInfo.
        /// Intended for the player controlling the creature, or the game master.
        /// </summary>
        public CreatureInfo ToCreatureInfo()
        {
            var _expiry = this.GetTake10Remaining(typeof(SkillBase));
            var _info = new CreatureInfo()
            {
                Abilities = Abilities.ToAbilitySetInfo(this),
                Age = Age,
                AgeInYears = AgeInYears,
                Alignment = Alignment.ToString(),
                BaseAttack = BaseAttack.ToDeltableInfo(),
                Body = Body.GetProviderInfo(null) as BodyInfo,
                CarryingCapacity = new CarryCapacityInfo
                {
                    HeavyLoadLimit = CarryingCapacity.HeavyLoadLimit,
                    LightLoadLimit = CarryingCapacity.LightLoadLimit,
                    MediumLoadLimit = CarryingCapacity.MediumLoadLimit,
                    LiftOffGroundLimit = CarryingCapacity.LoadLiftOffGround,
                    LiftOverHeadLimit = CarryingCapacity.LoadLiftOverHead,
                    PushOrDragLimit = CarryingCapacity.LoadPushDrag
                },
                Classes = new Collection<ClassInfo>(
                Classes.Select(_ac => _ac.ToClassInfo()).ToList()),
                Conditions = new Collection<string>(Conditions.Select(_c => _c.Display).Distinct().ToList()),
                CreatureType = CreatureType.Name,
                DamageReductions = new Collection<string>(DamageReductions.Where(_dr => _dr.Amount > 0).Select(_dr => _dr.Name).ToList()),
                Devotion = Devotion.Name,
                Encumberance = new EncumberanceInfo
                {
                    // encumerance
                    Unencumbered = EncumberanceCheck.Unencumbered,
                    Encumbered = EncumberanceCheck.Encumbered,
                    GreatlyEncumbered = EncumberanceCheck.GreatlyEncumbered,
                    // object load-based encumberance
                    NotWeighedDown = EncumberanceCheck.NotWeighedDown,
                    WeighedDown = EncumberanceCheck.WeighedDown,
                    Straining = EncumberanceCheck.Straining,
                    OverLoaded = EncumberanceCheck.AtlasMustShrug,
                    Value = EncumberanceCheck.Value
                },
                EnergyResistances = new Collection<string>(EnergyResistances.EffectiveResistances.Select(_er => _er.Description).ToList()),
                Feats = new Collection<FeatInfo>(
                Feats.Select(_f => _f.ToFeatInfo()).ToList()),
                FortitudeSave = FortitudeSave.ToDeltableInfo(),
                Gender = Gender.ToString(),
                HealthPoints = new HealthPointInfo
                {
                    Current = HealthPoints.CurrentValue,
                    DeadValue = HealthPoints.DeadValue.EffectiveValue,
                    Extra = HealthPoints.ExtraHealthPoints,
                    FromConstitution = HealthPoints.FromConstitution,
                    FromPowerDice = HealthPoints.HealthDieHealthPoints,
                    MassiveDamage = HealthPoints.MassiveDamage.EffectiveValue,
                    Maximum = HealthPoints.TotalValue,
                    NonLethal = HealthPoints.NonLethalDamage,
                    Temporary = TempHealthPoints.Total
                },
                IncorporealArmorRating = IncorporealArmorRating.ToDeltableInfo(),
                Initiative = Initiative.ToDeltableInfo(),
                Languages = new Collection<string>(Languages.Select(_l => _l.Name).ToList()),
                LoadWeight = ObjectLoad.Weight,
                MaxDexterityToArmorRating = MaxDexterityToARBonus.Value,
                MeleeDeltable = MeleeDeltable.ToDeltableInfo(),
                Movements = new Collection<MovementInfo>((
                from _mv in Movements.AllMovements
                let _mi = _mv.GetProviderInfo(null) as MovementInfo
                where _mi != null
                select _mi).ToList()),
                Name = Name,
                NormalArmorRating = NormalArmorRating.ToDeltableInfo(),
                OpposedDeltable = OpposedDeltable.ToDeltableInfo(),
                Proficiencies = Proficiencies.ProficiencyDescriptions,
                RangedDeltable = RangedDeltable.ToDeltableInfo(),
                ReflexSave = ReflexSave.ToDeltableInfo(),
                Senses = new Collection<string>(Senses.AllSenses.Select(_s => _s.Name).ToList()),
                BasicSkills = new Collection<SkillInfo>(
                Skills.BasicSkills.Select(_skill => _skill.ToSkillInfo(this)).ToList()),
                ParameterizedSkills = new Collection<SkillInfo>(
                Skills.ParameterizedSkills.Select(_skill => _skill.ToSkillInfo(this)).ToList()),
                SkillTake10 = _expiry != null
                ? new Take10Info { RemainingRounds = _expiry.Value }
                : null,
                Species = Species.Name,
                SpellResistance = SpellResistance.ToDeltableInfo(),
                SubTypes = new Collection<string>(SubTypes.Select(_st => _st.Name).ToList()),
                TouchArmorRating = TouchArmorRating.ToDeltableInfo(),
                Traits = new Collection<TraitInfo>((from _t in Traits
                                                    select new TraitInfo
                                                    {
                                                        Benefit = _t.Benefit,
                                                        Name = _t.Name,
                                                        TraitCategory = _t.TraitCategory.ToString(),
                                                        TraitNature = _t.TraitNature
                                                    }).ToList()),
                Weight = Weight, // NOTE: includes object load and body weight
                WillSave = WillSave.ToDeltableInfo(),
                ImageKeys = ImageKeys.ToArray()
            };
            return _info;
        }
        #endregion

        #region IProvideSaves
        public BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
            => (saveData?.SaveMode?.SaveType ?? SaveType.None) switch
            {
                SaveType.Fortitude => new BestSoftQualifiedDelta(FortitudeSave),
                SaveType.Reflex => new BestSoftQualifiedDelta(ReflexSave),
                SaveType.Will => new BestSoftQualifiedDelta(WillSave),
                _ => null,
            };

        public bool AlwaysFailsSave
            => false;
        #endregion

        protected override string ClassIconKey
            => nameof(Creature);

        #region public IVolatileValue GetIntrinsicPowerDifficulty(IPowerClass powerClass, string mnemonic, object source, IInteract target = null)
        /// <summary>Difficulty base for intrinsic powers that use 1/2 power dice</summary>
        public IVolatileValue GetIntrinsicPowerDifficulty(IPowerClass powerClass, string mnemonic, object source, IInteract target = null)
        {
            var _difficulty = new Deltable(10);
            var _ability = Abilities[mnemonic] as IDelta;
            _difficulty.Deltas.Add(new Delta(_ability.Value, _ability.Source));
            if (powerClass == null)
            {
                // no power class is sourced directly from creature power-level
                _difficulty.Deltas.Add(new Delta(
                    AdvancementLog.PowerLevel.QualifiedValue(new Qualifier(this, source, target)) / 2,
                    GetType()));
            }
            else
            {
                // otherwise use power class
                _difficulty.Deltas.Add(new Delta(
                    powerClass.ClassPowerLevel.QualifiedValue(new Qualifier(this, source, target)) / 2,
                    powerClass.GetType()));
            }
            return _difficulty;
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => new CreatureObjectInfo
            {
                // TODO: known specific creatures
                // TODO: species identification
                Message = Species.Name,
                BodySource = Body.SourceName(),
                Features = Body.Features
                    .OrderByDescending(_f => _f.IsMajor)
                    .Select(_f => _f.Description).ToArray(),
                Size = Sizer.Size.ToSizeInfo(),
                Portrait = GetPortraitInfo((Setting as LocalMap)?.Resources)
            };

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public AdvanceableCreature ToAdvanceableCreature()
            => new AdvanceableCreature
            {
                ID = ID,
                NumberPowerDice = AdvancementLog.NumberPowerDice,
                PowerDiceCount = AdvancementLog.PowerDiceCount,
                AdvancementLogInfos = AdvancementLog.Select(_log => _log.ToAdvancementLogInfo()).ToList()
            };

        public void OnDeserialization(object sender)
        {
            // NOTE: should have made conditions creature bound way-back in the day
            Conditions.SetCreature(this);
            _Sounds ??= new SoundAwarenessSet(this);
            _Rooms ??= new RoomAwarenessSet(this);
        }

        public Creature TemplateClone(string newName)
        {
            var _critter = new Creature(newName, Abilities.Clone())
            {
                Devotion = new Devotion(Devotion.Name),
                IsInSystemEditMode = true
            };

            // species
            var _species = Species.TemplateClone(_critter);
            _species.BindTo(_critter);

            // conformulate power dice
            if (_species.FractionalPowerDie < 1.0m)
            {
                // TODO: if first power die is not (smallest) fractional, but target is, need to adjust target
                // TODO: if first power die is smallest of fractional, but target is not, need to peel back power dice on target
            }

            // every advancement log item in template beyond current clone target
            foreach (var _adv in AdvancementLog
                .Where(_al => _al.StartLevel > _critter.AdvancementLog.PowerDiceCount).ToList())
            {
                // TODO: step increase level, fulfilling requirements identically
            }

            // TODO: possessions (clone and possess)
            // TODO: item slots and containment

            // finish and return
            _critter.IsInSystemEditMode = false;
            return _critter;
        }
    }
}
