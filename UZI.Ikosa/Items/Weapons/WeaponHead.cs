using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>Base class for generic WeaponTypeBoundHead&lt;&gt;</summary>
    [Serializable]
    public abstract class WeaponHead : CoreObject, IAttackSource, IDamageSource, IEnhancementTracker,
        IMonitorChange<Size>, IMonitorChange<DeltaValue>, IActionSource, IWeaponHead
    {
        #region Constructors
        protected WeaponHead(IWeapon weapon, string dmgRoll, DamageType[] dmgTypes, int criticalLow,
            DeltableQualifiedDelta criticalMultiplier, Materials.Material headMaterial,
            Lethality lethality)
            : base((weapon != null) ? weapon.Name : string.Empty)
        {
            _ContainingWeapon = weapon;
            _MediumDamage = dmgRoll;
            _DamageTypes = dmgTypes;
            _HeadMaterial = headMaterial;
            _AttackBonus = new DeltableQualifiedDelta(0, @"Enhancement", typeof(Enhancement));
            _DamageBonus = new DeltableQualifiedDelta(0, @"Enhancement", typeof(Enhancement));
            _TotalEnhancement = new ConstDeltable(0);
            _CriticalRange = 21 - criticalLow;
            _CriticalRangeFactor = new DeltableQualifiedDelta(1, @"Critical Range", typeof(WeaponHead));
            _CriticalDamageFactor = criticalMultiplier;
            if (ContainingWeapon != null)
                ContainingWeapon.ItemSizer.AddChangeMonitor(this);
            _DamageBonus.AddChangeMonitor(this);
            _CriticalRangeFactor.AddChangeMonitor(this);
            _TotalEnhancement.AddChangeMonitor(this);
            _LastPrice = 0;
            _Lethal = lethality;
        }

        protected WeaponHead(IWeapon weapon, string dmgRoll, DamageType dmgType, int criticalLow,
            DeltableQualifiedDelta criticalMultiplier, Materials.Material headMaterial,
            Lethality lethality)
            : this(weapon, dmgRoll, new DamageType[] { dmgType }, criticalLow, criticalMultiplier, headMaterial, lethality)
        {
        }

        protected WeaponHead(WeaponBase weapon, string dmgRoll, DamageType dmgType, Materials.Material headMaterial,
            Lethality lethality) :
            this(weapon, dmgRoll, dmgType, 20, new DeltableQualifiedDelta(2, @"Critical Damage", typeof(WeaponHead)),
           headMaterial, lethality)
        {
        }
        #endregion

        #region private/protected data
        protected string _MediumDamage;
        protected Materials.Material _HeadMaterial;
        protected DamageType[] _DamageTypes;
        protected IWeapon _ContainingWeapon;
        protected decimal _FixedSpecialCost;
        protected DeltableQualifiedDelta _AttackBonus;
        protected int _CriticalRange;
        protected DeltableQualifiedDelta _CriticalRangeFactor;
        protected DeltableQualifiedDelta _CriticalDamageFactor;
        protected Alignment _Alignment = Alignment.TrueNeutral;
        protected DeltableQualifiedDelta _DamageBonus;
        protected ConstDeltable _TotalEnhancement;
        protected decimal _LastPrice;
        protected Lethality _Lethal;
        #endregion

        #region Damage Rollers

        /// <summary>Get damage roller progression by size (default is use WeaponDamageRollers.DiceLookup[rollString])</summary>
        protected virtual Dictionary<int, Roller> RollerProgression()
            => WeaponDamageRollers.DiceLookup[MediumDamageRollString];

        #region protected IEnumerable<DamageRollPrerequisite> DeepBaseDamageRollers(Interaction attack, string keyFix)
        /// <summary>Needed for anonymous (iterator) base class calling</summary>
        protected IEnumerable<DamageRollPrerequisite> DeepBaseDamageRollers(Interaction attack, string keyFix, int minGroup)
        {
            var _atk = (attack.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // weapon base damage
            if (!MediumDamageRollString.Equals(@"-"))
            {
                if (ContainingWeapon != null)
                    yield return new DamageRollPrerequisite(this, attack, string.Concat(keyFix, @"Weapon"), MyName,
                        RollerProgression()[MySizer.EffectiveCreatureSize.Order], false, _nonLethal, @"Weapon", minGroup);

                // weapon damage bonuses
                var _wpnBonus = DamageBonus.QualifiedValue(attack);
                if (_wpnBonus != 0)
                    yield return new DamageRollPrerequisite(typeof(DeltableQualifiedDelta), attack, string.Concat(keyFix, @"Enhancement"), @"Enhancement",
                        new ConstantRoller(_wpnBonus), false, _nonLethal, @"Bonus", minGroup);

                // creature-based damage bonuses
                if ((MyPossessor != null) && (ContainingWeapon?.MainSlot != null))
                {
                    var _extraWpnDmg = MyPossessor.ExtraWeaponDamage.QualifiedValue(attack);
                    if (_extraWpnDmg != 0)
                        yield return new DamageRollPrerequisite(typeof(Creature), attack, string.Concat(keyFix, @"Creature"), @"Creature",
                            new ConstantRoller(_extraWpnDmg), false, _nonLethal, @"Creature", minGroup);
                }
            }
            yield break;
        }
        #endregion

        /// <summary>Returns all base damage rollers (those which can be piled on with criticals hits)</summary>
        public virtual IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction attack, string keyFix, int minGroup)
            => DeepBaseDamageRollers(attack, keyFix, minGroup);

        /// <summary>Returns all extra damage effects, not multiplied by critical hits</summary>
        public virtual IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction attack)
        {
            // extra damage effects due to the weapon...
            foreach (var _dmg in from _wed in this.ExtraDamages()
                                 where _wed.PoweredUp
                                 from _edr in _wed.DamageRollers(attack)
                                 select _edr)
                yield return _dmg;

            // extra damage due to creature
            foreach (var _dmg in GetDamageRollPrerequisites.GetDamageRollFeedback(MyPossessor, attack))
                yield return _dmg;
            yield break;
        }

        #region public virtual IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction attack)
        public virtual IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction attack)
        {
            // TODO: improved natural attack will catch size changes and bump the relative creature size (if possible)

            // base weapon damage
            foreach (var _roll in BaseDamageRollers(attack, string.Empty, 0))
                yield return _roll;

            if (attack != null)
            {
                if ((attack.Feedback.FirstOrDefault(_f => _f is AttackFeedback) is AttackFeedback _atkBack)
                    && _atkBack.CriticalHit)
                {
                    // extra rolls for critical
                    var _multiplier = CriticalDamageFactor.QualifiedValue(attack);
                    for (var _cx = 1; _cx < _multiplier; _cx++)
                    {
                        foreach (var _roll in BaseDamageRollers(attack, $@"Critical.{_cx}.", _cx))
                            yield return _roll;
                    }
                }
            }

            // extra damage effects due to the weapon...
            foreach (var _dmgRoll in ExtraDamageRollers(attack))
                yield return _dmgRoll;
            yield break;
        }
        #endregion

        #endregion

        #region Damage Roll Strings

        #region public string EffectiveDamageRollString(target)
        public string EffectiveDamageRollString(Interaction workSet)
        {
            var _build = new StringBuilder();
            foreach (var _roll in DamageRollers(workSet))
            {
                if (!(_roll.Roller is ConstantRoller))
                {
                    // non-constant rollers
                    _build.AppendFormat(@"{0}{1}", (_build.Length > 0 ? @"+" : string.Empty), _roll.ToString());
                }
                else
                {
                    // exclude 0 constants, negatives don't get a plus
                    var _val = (_roll.Roller as ConstantRoller).MaxRoll;
                    if (_val > 0)
                        _build.AppendFormat(@"{0}{1}", (_build.Length > 0 ? @"+" : string.Empty), _roll.ToString());
                    else if (_val < 0)
                        _build.Append(_roll.ToString());
                }

            }
            return _build.ToString();
        }
        #endregion

        #region public string MediumDamageRollString { get; set; }
        /// <summary>The damage for medium</summary>
        public string MediumDamageRollString
        {
            get { return _MediumDamage; }
            set { _MediumDamage = value; }
        }
        #endregion

        public string SizedDamageRollString
            => MediumDamageRollString.Equals(@"-") ? @"-" : RollerProgression()[MySizer.EffectiveCreatureSize.Order].ToString();

        public string CurrentDamageRollString
            => EffectiveDamageRollString(new Interaction(MyPossessor, this, null, null));

        #endregion

        #region public Materials.Material HeadMaterial {get;set;}
        public Materials.Material HeadMaterial
        {
            get { return _HeadMaterial; }
            set { _HeadMaterial = value; }
        }
        #endregion

        public IWeapon ContainingWeapon => _ContainingWeapon;

        #region public IEnumerable<ItemSlot> AssignedSlots { get; }
        /// <summary>
        /// If double weapon dual wielding, then only the slot for this head.  
        /// Otherwise, all slots.
        /// If double weapon two-hand wielding, then first slot is main sequence head.
        /// </summary>
        public IEnumerable<ItemSlot> AssignedSlots
        {
            get
            {
                if (ContainingWeapon.AllSlots.Count() == 1)
                {
                    // only slot
                    yield return ContainingWeapon.MainSlot;
                }
                else if (ContainingWeapon.GetWieldTemplate() == WieldTemplate.Double)
                {
                    var _dbl = ContainingWeapon as IMultiWeaponHead;
                    if (_dbl.IsDualWielding)
                    {
                        if (this == _dbl.MainHead)
                            yield return _dbl.MainSlot;
                        else
                            yield return _dbl.SecondarySlot;
                    }
                    else
                    {
                        // wielding without two hands
                        if (this == _dbl.MainHead)
                        {
                            yield return _dbl.MainSlot;
                            yield return _dbl.SecondarySlot;
                        }
                        else
                        {
                            yield return _dbl.SecondarySlot;
                            yield return _dbl.MainSlot;
                        }
                    }
                }
                else
                {
                    // all slots
                    foreach (var _slot in ContainingWeapon.AllSlots)
                        yield return _slot;
                }
                yield break;
            }
        }
        #endregion

        /// <summary>All assigned holding slots are off-hand slots</summary>
        public bool IsOffHand
            => AssignedSlots.All(_hs => _hs.IsOffHand);

        public virtual DamageType[] DamageTypes => _DamageTypes;

        /// <summary>Weapon head's enhancement to attack score</summary>
        public DeltableQualifiedDelta AttackBonus => _AttackBonus;

        /// <summary>Weapon head's enhancement to damage score</summary>
        public DeltableQualifiedDelta DamageBonus => _DamageBonus;

        public bool IsSourceChannel(IAttackSource source)
            => source == this;

        /// <summary>Used for cost calculation, includes enhancement costs for some special weapon abilities</summary>
        public ConstDeltable TotalEnhancement => _TotalEnhancement;

        /// <summary>Lists the enhancement bonus of the weapon head (excludes Masterwork enhancement to attack)</summary>
        public int ListedEnhancement
            => TotalEnhancement.Deltas[typeof(Enhancement)]?.Value ?? 0;

        #region public virtual decimal SpecialCost { get; }
        public virtual decimal SpecialCost
        {
            get
            {
                // TODO: determine cost for special materials
                return TotalEnhancement.EffectiveValue * TotalEnhancement.EffectiveValue * 2000m + _FixedSpecialCost;
            }
        }
        #endregion

        #region Critical Hit Parameters

        /// <summary>Depth of the Critical Range.</summary>
        public virtual int CriticalRange => _CriticalRange;

        /// <summary>Critical range factor (naturally 1, but may be enhanced to 2)</summary>
        public virtual DeltableQualifiedDelta CriticalRangeFactor => _CriticalRangeFactor;

        /// <summary>Normal effective critical multiplier</summary>
        public virtual DeltableQualifiedDelta CriticalDamageFactor => _CriticalDamageFactor;

        public int CriticalMultiplier
            => CriticalDamageFactor.QualifiedValue(new Interaction(MyPossessor, this, null, null));

        /// <summary>Normal effective critical low score</summary>
        public int CriticalLow
            => 21 - CriticalRange * CriticalRangeFactor.QualifiedValue(
                new Interaction(MyPossessor, this, null, null));

        #endregion

        public virtual Alignment Alignment
            => this.GetAlignment();

        public virtual bool IsMagicalDamage
            => this.IsEnhancedActive() || this.IsMagicWeaponActive();

        #region protected virtual void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        protected virtual void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        {
            // first, damage against the target
            var _atkFB = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _target = result.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            var _critter = result.AttackData.Attacker as Creature;
            if ((_atkFB != null) && (_atkFB.Hit) && (_target != null))
            {
                // collect damage
                var _damages = (from _dmgRoll in result.DispensedPrerequisites.OfType<DamageRollPrerequisite>()
                                from _dmg in _dmgRoll.GetDamageData()
                                select _dmg).ToList();

                // damage total
                var _deliverDamage = new DeliverDamageData(_critter, _damages, false, _atkFB.CriticalHit);
                if (secondaries.Any())
                    _deliverDamage.Secondaries.AddRange(secondaries);
                result.EnqueueNotify(
                    new DealingDamageNotify(_critter.ID, @"Dealing Damage", GetInfoData.GetInfoFeedback(this, _critter),
                    _damages.Select(_d => _d.ToDamageInfo()).ToList()),
                    _critter.ID);

                // damage interaction, and retry if demanded
                var _dmgInteract = new StepInteraction(result, _critter, this, _target.Target, _deliverDamage);
                _target.Target.HandleInteraction(_dmgInteract);
                if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
                {
                    new RetryInteractionStep(result, @"Retry", _dmgInteract);
                }
            }
        }
        #endregion

        #region public virtual void AttackResult(AttackResultStep result, Interaction workSet)
        /// <summary>Informs the attack source of the results of an attack.</summary>
        public virtual void AttackResult(AttackResultStep result, Interaction workSet)
        {
            // extra damage effects due to the weapon...
            var _sources = new List<object>();
            var _secondaries = new List<ISecondaryAttackResult>();
            foreach (var _processor in from _proc in Adjuncts.OfType<ISecondaryAttackResult>()
                                       where _proc.PoweredUp
                                       select _proc)
            {
                if (!_sources.Contains(_processor.AttackResultSource))
                {
                    _secondaries.Add(_processor);
                    _sources.Add(_processor.AttackResultSource);
                }
            }

            DamageAttackResult(result, workSet, _secondaries);
            result.AttackAction?.Attack?.AttackResultEffects(result, workSet);
        }

        #endregion

        #region public virtual IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        /// <summary>Gets prerequisites for an attack result</summary>
        public virtual IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        {
            var _unq = new List<string>();

            // first, damage against the target
            var _atkFB = attack.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if ((_atkFB != null) && (_atkFB.Hit))
            {
                #region damage rollers
                foreach (var _rollPre in DamageRollers(attack))
                {
                    // if the prerequisite assumes a unique key, check for existing
                    if (_rollPre.UniqueKey)
                    {
                        // if there are no matching bindKeys, add the prerequisite
                        if (!_unq.Contains(_rollPre.BindKey))
                            yield return _rollPre;
                    }
                    else
                        // add it!
                        yield return _rollPre;

                    // keys
                    _unq.Add(_rollPre.BindKey);
                }
                #endregion
            }

            #region non-damage prerequisites
            // extra prerequisites due to the weapon...
            var _sources = new List<object>(); ;
            foreach (var _processor in from _proc in Adjuncts.OfType<ISecondaryAttackResult>()
                                       where _proc.PoweredUp
                                       select _proc)
            {
                if (!_sources.Contains(_processor.AttackResultSource))
                {
                    _sources.Add(_processor.AttackResultSource);
                    foreach (var _pre in _processor.AttackResultPrerequisites(attack))
                    {
                        // if the prerequisite assumes a unique key, check for existing
                        if (_pre.UniqueKey)
                        {
                            // if there are no matching bindKeys, add the prerequisite
                            if (!_unq.Contains(_pre.BindKey))
                                yield return _pre;
                        }
                        else
                            // add it!
                            yield return _pre;

                        // keys
                        _unq.Add(_pre.BindKey);
                    }
                }
            }
            #endregion

            yield break;
        }
        #endregion

        #region IMonitorChange<Size> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args) { DoPropertyChanged(@"CurrentDamageRollString"); }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        public virtual void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public virtual void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public virtual void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (sender == _CriticalRangeFactor)
            {
                DoPropertyChanged(@"CriticalLow");
            }
            else if (sender == _DamageBonus)
            {
                DoPropertyChanged(@"CurrentDamageRollString");
            }
            else if (sender == _TotalEnhancement)
            {
                // cost of enhancements changed
                var _square = TotalEnhancement.EffectiveValue;
                _square *= _square;
                var _newPrice = 2000m * _square;

                // adjust base price cost
                ContainingWeapon.Price.BaseItemExtraPrice += (_newPrice - _LastPrice);
                _LastPrice = _newPrice;
            }
        }
        #endregion

        public override CoreSetting Setting
            => ContainingWeapon?.GetTokened()?.Token.Context.ContextSet.Setting;

        public bool IsMainHead
            => (ContainingWeapon as DoubleMeleeWeaponBase)?.MainHead == this;

        #region protected void DoCommonInfo(WeaponHeadInfo info, bool baseValues)
        protected void DoCommonInfo(WeaponHeadInfo info, bool baseValues)
        {
            info.DamageTypes = new Collection<DamageType>(DamageTypes.ToList());
            info.CriticalRange = CriticalRange;
            info.Material = HeadMaterial.ToMaterialInfo();
            info.IsMainHead = IsMainHead;

            // known and unknown values
            if (!baseValues)
            {
                info.CriticalRangeFactor = CriticalRangeFactor.ToDeltableInfo();
                info.CriticalDamageFactor = CriticalDamageFactor.ToDeltableInfo();
                info.CriticalLow = CriticalLow;
            }
            else
            {
                // TODO: account for known deltas (creature provided)...?
                info.CriticalRangeFactor = new DeltableInfo(CriticalRangeFactor.BaseValue);
                info.CriticalDamageFactor = new DeltableInfo(CriticalDamageFactor.BaseValue);
                info.CriticalLow = 21 - CriticalRange;
            }
            if (!baseValues)
            {
                info.AdjunctInfos = (from _adj in Adjuncts.OfType<IIdentification>()
                                     from _info in _adj.IdentificationInfos
                                     select _info).ToArray();
            }
            else
            {
                info.AdjunctInfos = new Info[] { };
            }
        }
        #endregion

        #region public virtual WeaponHeadInfo ToWeaponHeadInfo(CoreActor actor, bool baseValues)
        public virtual WeaponHeadInfo ToWeaponHeadInfo(CoreActor actor, bool baseValues)
        {
            var _info = new WeaponHeadInfo
            {
                ID = ID,
                Message = Name,
                DamageRollString = SizedDamageRollString
            };
            DoCommonInfo(_info, baseValues);
            return _info;
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ToWeaponHeadInfo(actor, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        protected virtual Creature MyPossessor
            => ContainingWeapon.CreaturePossessor;

        protected virtual ItemSizer MySizer
            => ContainingWeapon.ItemSizer;

        protected virtual string MyName
            => ContainingWeapon.GetKnownName(MyPossessor);

        public Lethality LethalitySelection => _Lethal;

        public override bool IsTargetable => false;
    };

    /// <summary>Weapon type parameterized WeaponHead derived class that allows checking feats.</summary>
    [Serializable]
    public class WeaponBoundHead<Wpn> : WeaponHead where Wpn : IWeapon
    {
        #region ctor
        public WeaponBoundHead(IWeapon weapon, string dmgRoll, DamageType[] dmgTypes, int criticalLow,
            int criticalMultiplier, Materials.Material headMaterial,
            Lethality lethality = Lethality.NormallyLethal)
            : base(weapon, dmgRoll, dmgTypes, criticalLow,
            new DeltableQualifiedDelta(criticalMultiplier, @"Multi", typeof(WeaponHead)), headMaterial,
            lethality)
        {
        }

        public WeaponBoundHead(IWeapon weapon, string dmgRoll, DamageType dmgType, int criticalLow,
            int criticalMultiplier, Materials.Material headMaterial,
            Lethality lethality = Lethality.NormallyLethal)
            : base(weapon, dmgRoll, dmgType, criticalLow,
            new DeltableQualifiedDelta(criticalMultiplier, @"Multi", typeof(WeaponHead)), headMaterial,
            lethality)
        {
        }

        public WeaponBoundHead(WeaponBase weapon, string dmgRoll, DamageType dmgType,
            Dictionary<int, Roller> customRollers, int criticalLow, int criticalMultiplier,
            Materials.Material headMaterial,
            Lethality lethality = Lethality.NormallyLethal)
            : base(weapon, dmgRoll, dmgType, criticalLow,
            new DeltableQualifiedDelta(criticalMultiplier, @"Multi", typeof(WeaponHead)), headMaterial,
            lethality)
        {
            _DamageRollers = customRollers;
        }
        #endregion

        private Dictionary<int, Roller> _DamageRollers = null;

        protected override Dictionary<int, Roller> RollerProgression()
            => _DamageRollers ?? base.RollerProgression();

        protected override string ClassIconKey
            => string.Empty;

        public override IEnumerable<string> IconKeys
            => ContainingWeapon?.IconKeys ?? new string[] { };
    }
}