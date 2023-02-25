using System;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Adjuncts;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    [Serializable]
    public class HealthPoints : ICreatureBound, IMonitorChange<DeltaValue>, IControlChange<HealthPointTotal>, IMonitorChange<PowerDiceCount>
    {
        #region Construction
        public HealthPoints(Creature creature)
        {
            _Creature = creature;
            _HealthDieHealthPoints = 0;
            _ExtraHealthPoints = 0;
            _NonLethal = 0;
            _NaturalHealTime = 0d;
            _ValueFromCon = RecalcFromCon();
            _Creature.Abilities.Constitution.AddChangeMonitor(this);
            _Creature.AdvancementLog.AddChangeMonitor(this);
            _ValueCtrlr = new ChangeController<HealthPointTotal>(this, new HealthPointTotal(0));
            DeadValue = new Deltable(-10);
            MassiveDamage = new Deltable(50);
        }
        #endregion

        #region data
        private int _DamageTaken;
        private int _NonLethal;
        private int _ExtraHealthPoints;
        private int _HealthDieHealthPoints;
        private Creature _Creature;
        private ChangeController<HealthPointTotal> _ValueCtrlr;
        private int _ValueFromCon;
        private double _NaturalHealTime;
        #endregion

        #region private void EnsureOnlyThisLimiter<Limiter>(Func<Limiter> generator)
        private void EnsureOnlyThisLimiter<Limiter>(Func<Limiter> generator)
            where Limiter : Adjunct, IHealthActivityLimiter
        {
            // get rid of any limiter that is not this type
            foreach (var _adj in (from _hl in Creature.Adjuncts.OfType<IHealthActivityLimiter>()
                                  let _a = _hl as Adjunct
                                  where !(_a is Limiter) && (_a.Source == this)
                                  select _a).ToList())
            {
                _adj.Eject();
            }

            // add limiter if it is not defined
            if (!Creature.HasAdjunct<Limiter>())
            {
                Creature.AddAdjunct(generator());
            }
        }
        #endregion

        #region private void EnsureNoLimiter()
        private void EnsureNoLimiter()
        {
            foreach (var _adj in (from _hl in Creature.Adjuncts.OfType<IHealthActivityLimiter>()
                                  let _a = _hl as Adjunct
                                  where (_a.Source == this)
                                  select _a).ToList())
            {
                _adj.Eject();
            }
        }
        #endregion

        #region public void DoRecovery()
        public void DoRecovery()
        {
            // any healing removes dying...
            var _dying = Creature.Adjuncts.OfType<Dying>().FirstOrDefault();
            if (_dying != null)
                _dying.Eject();

            var _value = CurrentValue;
            var _nonLethal = NonLethalDamage;
            var _disabled = Creature.Adjuncts.OfType<Disabled>().FirstOrDefault();

            // remove IRecoveryAdjuncts
            // - if (_value >= 0), remove all
            // - otherwise remove just the ones indicating the creature is barely recovering
            foreach (var _adj in (from _ira in Creature.Adjuncts.OfType<IRecoveryAdjunct>()
                                  where (_value >= 0) || _ira.IsBarelyRecovering
                                  select _ira as Adjunct).ToList())
            {
                _adj.Eject();
            }

            if (_value < 0)
            {
                // should be disabled or stable assisted now
                if (_disabled == null)
                {
                    // not disabled, add missing stable assisted
                    if (!Creature.HasAdjunct<StableAssisted>())
                        Creature.AddAdjunct(new StableAssisted(this));
                }
            }
            else if (_value == 0)
            {
                // should be disabled or non-lethal overload
                // NOTE: if already in disabled state, we assume we can stay in disabled state after healing
                //       this indicates that we were disabled and do not become unconscious after being healed
                if (_disabled == null)
                {
                    // not previously disabled, we can check for non-lethal overload
                    if (_nonLethal == 0)
                    {
                        // no non-lethal damage, disabled is the correct state
                        EnsureOnlyThisLimiter<Disabled>(() => new Disabled(this));
                    }
                    else
                    {
                        // non-Lethal overload (need to clear non lethal damage!)
                        EnsureOnlyThisLimiter<NonLethalOverload>(() => new NonLethalOverload(this));
                    }
                }
            }
            else // _value > 0
            {
                // should be nothing, staggered, or non-lethal overload now
                if (_value > _nonLethal)
                {
                    // no adverse conditions
                    EnsureNoLimiter();
                }
                else if (_value == _nonLethal)
                {
                    // staggered by non-lethal
                    EnsureOnlyThisLimiter<Staggered>(() => new Staggered(this));
                }
                else // _value < _nonLethal
                {
                    if (_disabled == null)
                    {
                        // unconscious by non-lethal
                        EnsureOnlyThisLimiter<NonLethalOverload>(() => new NonLethalOverload(this));
                    }
                    else
                    {
                        // substitute staggered
                        // otherwise we move from disabled to unconscious after healing
                        EnsureOnlyThisLimiter<Staggered>(() => new Staggered(this));
                    }
                }
            }
        }
        #endregion

        #region private void AdjustDyingForDamage(bool dying)
        private void AdjustDyingForDamage(bool dying)
        {
            if (dying)
            {
                if (!Creature.HasAdjunct<Dying>())
                    Creature.AddAdjunct(new Dying(this));
            }
            else
            {
                var _dying = Creature.Adjuncts.OfType<Dying>().FirstOrDefault();
                if (_dying != null)
                    _dying.Eject();
            }
        }
        #endregion

        #region public void DoDamage()
        public void DoDamage()
        {
            // NOT STABLE, remove all recovery adjuncts
            foreach (var _adj in Creature.Adjuncts.OfType<IRecoveryAdjunct>()
                .Select(_ie => _ie as Adjunct).ToList())
            {
                _adj.Eject();
            }

            // just took some damage!
            var _value = CurrentValue;
            var _nonLethal = NonLethalDamage;
            if (_value > 0)
            {
                // not dying!
                AdjustDyingForDamage(false);

                if (_value > _nonLethal)
                {
                    // no adverse conditions
                    EnsureNoLimiter();
                }
                else if (_value == _nonLethal)
                {
                    // staggered by non-lethal
                    EnsureOnlyThisLimiter<Staggered>(() => new Staggered(this));
                }
                else // value < _NonLethal
                {
                    // unconscious by non-lethal
                    EnsureOnlyThisLimiter<NonLethalOverload>(() => new NonLethalOverload(this));
                }
            }
            else if (_value == 0)
            {
                // not dying
                AdjustDyingForDamage(false);

                if (_nonLethal == 0)
                {
                    EnsureOnlyThisLimiter<Disabled>(() => new Disabled(this));
                }
                else if (_nonLethal > _value)
                {
                    // unconscious by non-lethal
                    EnsureOnlyThisLimiter<NonLethalOverload>(() => new NonLethalOverload(this));
                }
            }
            else if (_value <= DeadValue.EffectiveValue)
            {
                // not dying
                AdjustDyingForDamage(false);

                EnsureNoLimiter();

                // dead effect (only dead once)
                var _dead = Creature.Adjuncts.OfType<DeadEffect>().FirstOrDefault();
                if (_dead == null)
                {
                    Creature.AddAdjunct(new DeadEffect(this, Creature?.GetCurrentTime() ?? 0));
                }
            }
            else // value < 0
            {
                // dying
                AdjustDyingForDamage(true);
                EnsureNoLimiter();
            }
        }
        #endregion

        #region PowerLevel and Constitution Changed
        private int RecalcFromCon()
            => Creature.Abilities.Constitution.IModifier.Value * Creature.AdvancementLog.NumberPowerDice;

        #region IMonitorChange<PowerDiceCount> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<PowerDiceCount> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<PowerDiceCount> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<PowerDiceCount> args)
        {
            var _old = _ValueFromCon;
            _ValueFromCon = RecalcFromCon();

            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(FromConstitution)));
            DoValueChanged();

            // treat alterations as damage and recovery
            if (_old > _ValueFromCon)
            {
                DoDamage();
            }
            else if (_old < _ValueFromCon)
            {
                DoRecovery();
            }
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }
        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            var _old = _ValueFromCon;
            _ValueFromCon = RecalcFromCon();

            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(FromConstitution)));
            DoValueChanged();

            // treat alterations as damage and recovery
            if (_old > _ValueFromCon)
            {
                DoDamage();
            }
            else if (_old < _ValueFromCon)
            {
                DoRecovery();
            }
        }
        #endregion
        #endregion

        #region public int CurrentValue { get; set; }
        /// <summary>Current hit point total</summary>
        public int CurrentValue
        {
            get => TotalValue - _DamageTaken;
            set
            {
                var _lastValue = CurrentValue;

                // "cap" value at Dead Value
                var _deadValue = DeadValue.EffectiveValue;
                var _newValue = value;
                if (_newValue < _deadValue)
                    _newValue = _deadValue;

                _DamageTaken = TotalValue - _newValue;

                // do not go into negative damage (temporary hit points are separate entities)
                if (_DamageTaken < 0) _DamageTaken = 0;

                if ((_lastValue - _newValue) >= MassiveDamage.EffectiveValue)
                {
                    if (!Creature.Abilities.Constitution.IsNonAbility)
                    {
                        // TODO: enqueue Massive damage save or die step?
                    }
                }

                if (_newValue <= _deadValue)
                {
                    // not dying (anymore!)
                    var _dying = Creature.Adjuncts.OfType<Dying>().FirstOrDefault();
                    if (_dying != null)
                        _dying.Eject();

                    // no action limiters (dead is dead!)
                    EnsureNoLimiter();

                    // dead effect (only dead once)
                    var _dead = Creature.Adjuncts.OfType<DeadEffect>().FirstOrDefault();
                    if (_dead == null)
                    {
                        Creature.AddAdjunct(new DeadEffect(this, Creature?.GetCurrentTime() ?? 0));
                    }
                }
            }
        }
        #endregion

        /// <summary>Value at which the creature's hit points make it dead (typically -10)</summary>
        public Deltable DeadValue { get; private set; }

        /// <summary>Single Damage drop at which save or die is needed (default = 50)</summary>
        public Deltable MassiveDamage { get; private set; }

        public int NonLethalDamage
        {
            get => _NonLethal;
            set
            {
                _NonLethal = value;
                CurrentValue = CurrentValue;
            }
        }

        public double LastNaturalHealTime { get => _NaturalHealTime; set => _NaturalHealTime = value; }

        // TODO: ImprovedToughness (accumulate on PowerDie added, subtract on HitHie decreased)

        #region public int ExtraHealthPoints { get; set; }
        /// <summary>Toughness (and other non hit-dice associated) accumulator</summary>
        public int ExtraHealthPoints
        {
            get => _ExtraHealthPoints;
            internal set
            {
                var _old = _ExtraHealthPoints;
                _ExtraHealthPoints = value;

                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ExtraHealthPoints)));
                DoValueChanged();

                // treat alterations as damage and recovery
                if (_old > _ExtraHealthPoints)
                {
                    DoDamage();
                }
                else if (_old < _ExtraHealthPoints)
                {
                    DoRecovery();
                }
            }
        }
        #endregion

        #region public int HealthDieHealthPoints { get; set; }
        /// <summary>Hit-Dice roll accumulator</summary>
        public int HealthDieHealthPoints
        {
            get => _HealthDieHealthPoints;
            internal set
            {
                // only used by creature editor
                _HealthDieHealthPoints = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(HealthDieHealthPoints)));
                DoValueChanged();
            }
        }
        #endregion

        public int FromConstitution => _ValueFromCon;

        public int TotalValue
            => _ValueFromCon + _HealthDieHealthPoints + _ExtraHealthPoints;

        // ICreatureBound Members
        public Creature Creature => _Creature;

        #region ValueChanged Event
        private void DoValueChanged()
        {
            _ValueCtrlr.DoValueChanged(new HealthPointTotal(TotalValue));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(TotalValue)));
        }

        #region IControlChange<HealthPointTotal> Members
        public void AddChangeMonitor(IMonitorChange<HealthPointTotal> subscriber)
        {
            _ValueCtrlr.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<HealthPointTotal> subscriber)
        {
            _ValueCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>Indicates the number of health points the creature has.</summary>
    [Serializable]
    public readonly struct HealthPointTotal
    {
        public HealthPointTotal(int total)
        {
            Total = total;
        }

        public readonly int Total;
    }
}
