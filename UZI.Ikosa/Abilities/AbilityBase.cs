using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Uzi.Core;
using Uzi.Ikosa.Time;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Abilities
{
    // TODO: 0 scores
    // A character with Strength 0 falls to the ground and is helpless. 
    // A character with Dexterity 0 is paralyzed. 
    // A character with Constitution 0 is dead. 
    // A character with Intelligence, Wisdom, or Charisma 0 is unconscious. 
    [Serializable]
    public abstract class AbilityBase : Deltable, IModifier, IControlChange<AbilityDamageValue>, ISupplyQualifyDelta
    {
        #region Construction
        public AbilityBase(int seedValue, string mnemonic)
            : base(seedValue)
        {
            _Mnemonic = mnemonic;
            _Term = new TerminateController(this);
            _PowerBoosts = new SortedList<int, int>();
            _ADCtrl = new ChangeController<AbilityDamageValue>(this, new AbilityDamageValue { Mnemonic = mnemonic, Source = this, Value = 0 });
            _CheckQual = new DeltableQualifiedDelta(0, @"Check", this);
        }

        /// <summary>
        /// Nonability constructor
        /// </summary>
        public AbilityBase(string mnemonic)
            : base(10)
        {
            _NonAbility = true;
            _Mnemonic = mnemonic;
            _Term = new TerminateController(this);
            _PowerBoosts = new SortedList<int, int>();
            _ADCtrl = new ChangeController<AbilityDamageValue>(this, new AbilityDamageValue { Mnemonic = mnemonic, Source = this, Value = 0 });
            _CheckQual = new DeltableQualifiedDelta(0, @"Check", this);
        }
        #endregion

        #region data
        private AbilitySet _Abilities;
        private DeltableQualifiedDelta _CheckQual;
        protected bool _NonAbility;
        private List<object> _Zeros;
        protected string _Mnemonic;

        // boosts
        private SortedList<int, int> _PowerBoosts;

        // damage
        private Delta _Dmg = null;
        private ChangeController<AbilityDamageValue> _ADCtrl;

        private TerminateController _Term;
        #endregion

        public IModifier IModifier => this;
        public DeltableQualifiedDelta CheckQualifiers => _CheckQual;
        public string Mnemonic => _Mnemonic;

        public override string ToString()
            => GetType().Name;

        public string DisplayValue
            => (IsNonAbility ? @"-" : EffectiveValue.ToString());

        /// <summary>Math.Floor(Value-10)/2</summary>
        public int DeltaValue => IModifier.Value;

        public AbilitySet Abilities
        {
            get { return _Abilities; }
            set { if (_Abilities == null) _Abilities = value; }
        }

        #region public bool IsNonAbility { get; }
        public bool IsNonAbility
        {
            get { return _NonAbility; }
            set
            {
                _NonAbility = value;
                DoValueChanged();
            }
        }
        #endregion

        public void SetZeroHold(object source, bool hold)
        {
            _Zeros ??= new List<object>();
            if (hold && !_Zeros.Contains(source))
            {
                _Zeros.Add(source);
                DoValueChanged();
            }
            else if (!hold && _Zeros.Contains(source))
            {
                _Zeros.Remove(source);
                DoValueChanged();
            }
        }

        public bool IsHeldAtZero
            => _Zeros?.Any() ?? false;

        #region IDelta Members
        int IDelta.Value
            => _NonAbility
            ? 0
            : (int)Math.Floor((EffectiveValue - 10.0M) / 2.0M);

        object ISourcedObject.Source
            => GetType();

        string IDelta.Name
            => GetType().Name;

        bool IDelta.Enabled { get { return true; } set { } }
        #endregion

        #region Power Boosts
        public virtual void Boost(int powerLevel)
        {
            if (!_PowerBoosts.ContainsKey(powerLevel))
            {
                _PowerBoosts.Add(powerLevel, powerLevel);
                BaseValue++;
            }
        }

        public virtual void Unboost(int powerLevel)
        {
            if (_PowerBoosts.Keys.Contains(powerLevel))
            {
                BaseValue--;
                _PowerBoosts.Remove(powerLevel);
            }
        }

        public IEnumerable<int> Boosts
        {
            get
            {
                foreach (KeyValuePair<int, int> _boost in _PowerBoosts)
                {
                    yield return _boost.Value;
                }
                yield break;
            }
        }
        #endregion

        /// <summary>Returns the effective value that would be valid at a particular power die level</summary>
        public int ValueAtPowerLevel(int powerLevel, Interaction interact)
            => IsNonAbility
            ? -1
            : IsHeldAtZero
            ? 0
            : QualifiedValue(interact) - _PowerBoosts.Keys.Count(_pdLevel => _pdLevel > powerLevel);

        protected override int CalcEffectiveValue()
            => _NonAbility
            ? -1
            : IsHeldAtZero
            ? 0
            : base.CalcEffectiveValue();

        #region protected override void DoValueChanged()
        protected override void DoValueChanged()
        {
            base.DoValueChanged();
            if ((Abilities != null) && Abilities.Constitution.IsNonAbility)
            {
                // no CON means no ability drain
                var _drains = Deltas.Where(_d => (_d.Source as Type) == typeof(Drain)).ToList();
                foreach (var _d in _drains)
                    Deltas.Remove(_d);
            }
            DoPropertyChanged(@"DisplayValue");
            DoPropertyChanged(@"DeltaValue");
        }
        #endregion

        #region Ability Damage Section

        /// <summary>Provides the delta used to track ability damage</summary>
        public Delta Damage => _Dmg;

        #region public void AddDamage(int amount, object source)
        /// <summary>Adds ability damage</summary>
        public void AddDamage(int amount, object source)
        {
            // no damage for non-living
            if ((Abilities != null) && Abilities.Constitution.IsNonAbility)
                return;
            if (!_ADCtrl.WillAbortChange(new AbilityDamageValue { Mnemonic = Mnemonic, Source = source, Value = amount }, @"PreAdd"))
            {
                if (_Dmg == null)
                {
                    _Dmg = new Delta(0 - amount, this, @"Ability Damage");
                    Deltas.Add(_Dmg);
                }
                else
                {
                    _Dmg.Value -= amount;
                }
            }
        }
        #endregion

        #region public void RecoverDamage(int amount)
        /// <summary>Recover ability damage.  Also, may remove the delta is fully recovered.</summary>
        public void RecoverDamage(int amount)
        {
            // no damage for non-living
            if ((Abilities != null) && Abilities.Constitution.IsNonAbility)
                return;
            if (!_ADCtrl.WillAbortChange(new AbilityDamageValue { Mnemonic = Mnemonic, Source = this, Value = amount }, @"PreRemove"))
            {
                if (_Dmg != null)
                {
                    _Dmg.Value += amount;
                    if (_Dmg.Value >= 0)
                    {
                        _Dmg.DoTerminate();
                        _Dmg = null;
                    }
                }
            }
        }
        #endregion

        #region public void RemoveDamage(int amount, object source)
        /// <summary>
        /// Remove ability damage.  Also, may remove the delta if fully recovered.  
        /// Does not update the recovery time, as this is intended to be used for magical recovery.
        /// </summary>
        public void RemoveDamage(int amount, object source)
        {
            // no damage for non-living
            if ((Abilities != null) && Abilities.Constitution.IsNonAbility)
                return;
            if (!_ADCtrl.WillAbortChange(new AbilityDamageValue { Mnemonic = Mnemonic, Source = source, Value = amount }, @"PreRemove"))
            {
                if (_Dmg != null)
                {
                    _Dmg.Value += amount;
                    if (_Dmg.Value >= 0)
                    {
                        _Dmg.DoTerminate();
                        _Dmg = null;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Checks
        /// <summary>Returns true if an ability check matches or beats the difficulty</summary>
        public bool AbilityCheck(Creature critter, int difficulty, ICore opposed, int rollValue, DeltaCalcInfo info)
        {
            // inform master of difficulty
            Deltable.GetDeltaCalcNotify(null, $@"{Mnemonic} Check Difficulty").DeltaCalc.SetBaseResult(difficulty);

            return !IsNonAbility && !IsHeldAtZero
                && CheckValue(new Qualifier(critter, opposed, critter), rollValue, info) >= difficulty;
        }

        /// <summary>Returns true if an automatic ability check matches or beats the difficulty</summary>
        public bool AutoCheck(Creature critter, int difficulty, ICore opposed, DeltaCalcInfo info)
            => AbilityCheck(critter, difficulty, opposed, DiceRoller.RollDie(critter.ID, 20, Mnemonic, $@"{ToString()} Check", critter.ID),
                info);

        /// <summary>Uses QualifiedDelta and CheckQualifiers.QualifiedValue</summary>
        public int? CheckValue(Qualifier workSet, int rollValue, DeltaCalcInfo info)
        {
            info.BaseValue = rollValue;
            if (IsNonAbility || IsHeldAtZero)
            {
                info.Result = 0;
                return null;
            }

            var _deltas = CheckQualifiers.QualifiedDeltas(workSet).Union(QualifiedDeltas(workSet)).Where(_d => _d.Value != 0).ToList();
            foreach (var _del in _deltas)
            {
                info.AddDelta(_del.Name, _del.Value);
            }
            info.Result = rollValue + _deltas.Sum(_c => _c.Value);
            return info.Result;
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term?.TerminateSubscriberCount ?? 0;
        #endregion
        #endregion

        #region IControlChange<AbilityDamageValue> Members
        public void AddChangeMonitor(IMonitorChange<AbilityDamageValue> monitor)
        {
            _ADCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<AbilityDamageValue> monitor)
        {
            _ADCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region ISupplyQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            var _val = IsNonAbility
                ? 0
                : IsHeldAtZero
                ? -5
                : (int)Math.Floor((QualifiedValue(qualify) - 10.0M) / 2.0M);
            yield return new QualifyingDelta(_val, GetType(), GetType().Name);
            yield break;
        }

        #endregion

        public AbilityInfo ToAbilityInfo(Creature critter)
        {
            var _info = ToInfo<AbilityInfo>(null);
            _info.Name = ((IDelta)this).Name;
            _info.Mnemonic = Mnemonic;
            _info.DisplayValue = DisplayValue;
            _info.DeltaValue = DeltaValue;
            _info.IsNonAbility = IsNonAbility;
            _info.Damage = Damage?.Value ?? 0;
            _info.Boosts = new Collection<int>(Boosts.ToList());
            var _expiry = critter.GetTake10Remaining(GetType());
            _info.Take10 = _expiry != null
                ? new Take10Info { RemainingRounds = _expiry.Value }
                : null;
            return _info;
        }

        public int Unboosted()
            => BaseValue - _PowerBoosts.Count;
    }

    [Serializable]
    public abstract class CastingAbilityBase : AbilityBase
    {
        public CastingAbilityBase(int seedValue, string mnemonic)
            : base(seedValue, mnemonic)
        {
        }

        /// <summary>Nonability constructor</summary>
        public CastingAbilityBase(string mnemonic)
            : base(mnemonic)
        {
        }

        /// <summary>Maximum spell level for this ability score (effective value - 10)</summary>
        public int MaxSpellLevel()
            => EffectiveValue - 10;

        #region public int BonusSpellsForLevel(int level)
        public int BonusSpellsForLevel(int level)
        {
            //-- Must always round down --
            var _bonus = (EffectiveValue - 2 - (2 * level)) / 8;
            return ((_bonus >= 0) ? _bonus : 0);
        }
        #endregion
    }
}
