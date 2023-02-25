using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class AttackTriggerable : Mechanism, ITriggerable, IAttackSource, IDamageSource
    {
        #region ctor()
        protected AttackTriggerable(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            _BaseAttack = new Delta(10, this, @"Base Attack");
            _AtkBonus = new DeltableQualifiedDelta(0, @"Attack Bonus", this);
            _AtkBonus.Deltas.Add(_BaseAttack);

            _BaseCriticalDamage = new Delta(2, this, @"Base Critical Damage");
            _CritDmgFactor = new DeltableQualifiedDelta(0, @"Critical Damage Factor", this);
            _CritDmgFactor.Deltas.Add(_BaseCriticalDamage);

            _CriticalRange = 1;

            _CritRangeFactor = new DeltableQualifiedDelta(1, @"Critical Range Factor", this);

            _BaseDamage = new Delta(0, this, @"Base Damage");
            _DmgBonus = new DeltableQualifiedDelta(0, @"Damage Bonus", this);
            _DmgBonus.Deltas.Add(_BaseDamage);

            _Dice = new DiceRoller(1, 6);
            _DamageType = DamageType.Slashing;
            _PostState = PostTriggerState.AutoReset;

            _Direct = false;
            AddAdjunct(new TrapPart(true));
        }
        #endregion

        #region data
        private DiceRoller _Dice;
        private int _CriticalRange;
        private Delta _BaseAttack;
        private Delta _BaseDamage;
        private Delta _BaseCriticalDamage;
        private DeltableQualifiedDelta _AtkBonus;
        private DeltableQualifiedDelta _DmgBonus;
        private DeltableQualifiedDelta _CritDmgFactor;
        private DeltableQualifiedDelta _CritRangeFactor;
        private bool _Direct;
        private DamageType _DamageType;
        private PostTriggerState _PostState;
        #endregion

        #region public string AttackInfo { get; }
        public string AttackInfo
        {
            get
            {
                var _dice = DamageDice?.Number > 0 ? DamageDice.ToString() : string.Empty;
                var _extra = DamageBonus.EffectiveValue > 0 ? $@"+{DamageBonus.EffectiveValue}" : string.Empty;
                var _crit = CriticalLow < 20 ? $@"/{CriticalLow}-20" : string.Empty;
                var _cDmg = CriticalDamageFactor.EffectiveValue > 2 ? $@"×{CriticalDamageFactor.EffectiveValue}" : string.Empty;
                return $@"{AttackBonus.EffectiveValue}: {_dice}{_extra}{_crit}{_cDmg} {DamageType}";
            }
        }
        #endregion

        #region public int CriticalRange { get; set; }
        public int CriticalRange
        {
            get => _CriticalRange;
            set
            {
                _CriticalRange = value;
                DoPropertyChanged(nameof(CriticalRange));
                DoPropertyChanged(nameof(AttackInfo));
            }
        }
        #endregion

        public PostTriggerState PostTriggerState
        {
            get => _PostState;
            set
            {
                _PostState = value;
                DoPropertyChanged(nameof(PostTriggerState));
            }
        }

        #region public void DoPostTrigger()
        public void DoPostTrigger()
        {
            switch (PostTriggerState)
            {
                case PostTriggerState.Destroy:
                    StructurePoints = 0;
                    break;

                case PostTriggerState.Damage:
                    // NOTE: implicit disable due to damage
                    StructurePoints = 1;
                    break;

                case PostTriggerState.Disable:
                    if (!this.HasAdjunct<DisabledObject>())
                        AddAdjunct(new DisabledObject());
                    break;

                case PostTriggerState.DeActivate:
                    // NOTE: no implicit re-activation action for a trigger
                    //       must attach an activation mechanism to re-enable the trigger
                    //       or the specific trigger must provide an action itself
                    Activation = new Activation(this, false);
                    break;

                default:
                    // includes AutoReset
                    break;
            }
        }
        #endregion

        public Delta BaseAttack => _BaseAttack;
        public Delta BaseDamage => _BaseDamage;
        public Delta BaseCriticalDamage => _BaseCriticalDamage;

        public DeltableQualifiedDelta AttackBonus => _AtkBonus;
        public DeltableQualifiedDelta DamageBonus => _DmgBonus;
        public DeltableQualifiedDelta CriticalRangeFactor => _CritRangeFactor;
        public DeltableQualifiedDelta CriticalDamageFactor => _CritDmgFactor;

        public Alignment Alignment => Alignment.TrueNeutral;

        public bool IsMagicalDamage
            => this.IsEnhancedActive() || this.IsMagicWeaponActive();

        #region protected IEnumerable<DamageRollPrerequisite> DeepBaseDamageRollers(Interaction attack, string keyFix)
        /// <summary>Needed for anonymous (iterator) base class calling</summary>
        protected IEnumerable<DamageRollPrerequisite> DeepBaseDamageRollers(Interaction attack, string keyFix, int minGroup)
        {
            var _atk = (attack.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // weapon base damage
            if (DamageDice?.Number > 0)
                yield return new DamageRollPrerequisite(this, attack, $@"{keyFix}Mechanism", Name,
                    DamageDice, false, _nonLethal, @"Weapon", minGroup);

            // weapon damage bonuses
            var _wpnBonus = DamageBonus.QualifiedValue(attack);
            if (_wpnBonus != 0)
                yield return new DamageRollPrerequisite(typeof(DeltableQualifiedDelta), attack,
                    string.Concat(keyFix, @"Enhancement"), @"Enhancement", new ConstantRoller(_wpnBonus), false, _nonLethal, @"Bonus", minGroup);
            yield break;
        }
        #endregion

        /// <summary>Returns all base damage rollers (those which can be piled on with criticals hits)</summary>
        public virtual IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction attack, string keyFix, int minGroup)
            => DeepBaseDamageRollers(attack, keyFix, minGroup);

        /// <summary>Returns all extra damage effects, not multiplied by critical hits</summary>
        public virtual IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction attack)
            => from _wed in this.ExtraDamages()
               where _wed.PoweredUp
               from _edr in _wed.DamageRollers(attack)
               select _edr;

        #region public override void FailedDisable(CoreActivity activity)
        public override void FailedDisable(CoreActivity activity)
        {
            base.FailedDisable(activity);

            // fail, trigger
            var _loc = activity?.Actor?.GetLocated()?.Locator;
            if (_loc != null)
            {
                DoTrigger(this, _loc.ToEnumerable());
            }
            else
            {
                DoTrigger(this, new List<Locator>());
            }
        }
        #endregion

        #region public virtual IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction attack)
        public virtual IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction attack)
        {
            // base weapon damage
            foreach (var _roll in BaseDamageRollers(attack, string.Empty, 0))
                yield return _roll;

            if ((attack?.Feedback.OfType<AttackFeedback>().FirstOrDefault() is AttackFeedback _atkBack)
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

            // extra damage effects due to the weapon...
            foreach (var _dmgRoll in ExtraDamageRollers(attack))
                yield return _dmgRoll;
            yield break;
        }
        #endregion

        /// <summary>Normal effective critical low score</summary>
        public int CriticalLow
            => 21 - CriticalRange * CriticalRangeFactor.QualifiedValue(
                new Interaction(null, this, null, null));

        #region public bool IsDirect { get; set; }
        /// <summary>If IsDirect, then attack targets triggering locator(s).  Useful for touch triggers.</summary>
        public bool IsDirect
        {
            get => _Direct;
            set
            {
                _Direct = value;
                DoPropertyChanged(nameof(IsDirect));
            }
        }
        #endregion

        #region public DamageType DamageType { get; set; }
        public DamageType DamageType
        {
            get => _DamageType;
            set
            {
                _DamageType = value;
                DoPropertyChanged(nameof(DamageType));
                DoPropertyChanged(nameof(AttackInfo));
            }
        }
        #endregion

        #region public DiceRoller DamageDice { get; set; }
        public DiceRoller DamageDice
        {
            get => _Dice;
            set
            {
                _Dice = value;
                DoPropertyChanged(nameof(DamageDice));
                DoPropertyChanged(nameof(AttackInfo));
            }
        }
        #endregion

        public DamageType[] DamageTypes => new DamageType[] { _DamageType };

        public string MediumDamageRollString
        {
            get => _Dice.ToString();
            set => DoPropertyChanged(nameof(MediumDamageRollString));
        }

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

        public override IEnumerable<IActivatable> Dependents
        {
            get
            {
                yield break;
            }
        }

        public ICellLocation Location
            => this.GetLocated()?.Locator.GeometricRegion.AllCellLocations().FirstOrDefault();

        #region public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        /// <summary>Gets prerequisites for an attack result</summary>
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        {
            var _unq = new List<string>();

            // first, damage against the target
            var _atkFB = attack.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (_atkFB?.Hit ?? false)
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
            // extra prerequisites due to the mechanism...
            var _sources = new List<object>();
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

        #region protected virtual void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        protected virtual void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        {
            // first, damage against the target
            var _atkFB = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _target = result.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            if ((_atkFB != null) && (_atkFB.Hit) && (_target != null))
            {
                // collect damage
                var _damages = (from _dmgRoll in result.DispensedPrerequisites.OfType<DamageRollPrerequisite>()
                                from _dmg in _dmgRoll.GetDamageData()
                                select _dmg).ToList();

                // damage total
                var _deliverDamage = new DeliverDamageData(null, _damages, false, _atkFB.CriticalHit);
                if (secondaries.Any())
                    _deliverDamage.Secondaries.AddRange(secondaries);

                // damage interaction, and retry if demanded
                var _dmgInteract = new StepInteraction(result, null, this, _target.Target, _deliverDamage);
                _target.Target.HandleInteraction(_dmgInteract);
                if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
                {
                    new RetryInteractionStep(result, @"Retry", _dmgInteract);
                }
            }
        }
        #endregion

        #region public void AttackResult(AttackResultStep result, Interaction workSet)
        public void AttackResult(AttackResultStep result, Interaction workSet)
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
        }
        #endregion

        public bool IsSourceChannel(IAttackSource source)
            => (source == this);

        public abstract void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators);
    }
}
