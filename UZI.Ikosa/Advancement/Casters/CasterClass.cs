using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class CasterClass : CharacterClass, ICasterClass
    {
        #region ctor()
        protected CasterClass(byte powerDie)
            : this(powerDie, PowerDieCalcMethod.Average)
        {
        }

        protected CasterClass(byte powerDie, PowerDieCalcMethod calcMethod)
            : base(powerDie, calcMethod)
        {
            _PowerLevel = new DeltableQualifiedDelta(0, @"Class Power Level", this);
            _PowerLevel.Deltas.Add(_LevelDelta);
            _ActCtrl = new ChangeController<Activation>(this, new Activation(this, true));
            _TCtrl = new TerminateController(this);
        }
        #endregion

        #region data
        private DeltableQualifiedDelta _PowerLevel;
        private ChangeController<Activation> _ActCtrl;
        #endregion

        /// <summary>Caster level checks and level-dependent spell effects</summary>
        public IVolatileValue ClassPowerLevel => _PowerLevel;

        /// <summary>Arcane or divine caster</summary>
        public abstract MagicType MagicType { get; }

        public Alignment Alignment
            => Creature.Alignment;

        public abstract string ClassIconKey { get; }
        public abstract CastingAbilityBase SpellDifficultyAbility { get; }
        public abstract CastingAbilityBase BonusSpellAbility { get; }
        public abstract IDeltable SpellDifficultyBase { get; }
        public virtual bool CanUseDescriptor(Descriptor descriptor) => true;

        #region add/remove class
        protected override void OnAdd()
        {
            base.OnAdd();
            _PowerLevel.Deltas.Add((IQualifyDelta)Creature.ExtraClassPowerLevel);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _PowerLevel.Deltas.Remove((IQualifyDelta)Creature.ExtraClassPowerLevel);

            // get rid of any self-suppressions
            foreach (var _pcs in Creature.Adjuncts.OfType<PowerClassSuppress>()
                .Where(_pcs => _pcs.Source.Equals(this)).ToList())
                _pcs.Eject();

            // and anything suppressing this
            foreach (var _pcs in Creature.Adjuncts.OfType<PowerClassSuppress>()
                .Where(_pcs => _pcs.PowerClass.Equals(this)).ToList())
                _pcs.Eject();

        }
        #endregion

        public Guid OwnerID
            => Creature?.ID ?? Guid.Empty;

        public abstract Type CasterClassType { get; }

        /// <summary>All spells that can be used (theoretically) by the caster.</summary>
        public abstract IEnumerable<ClassSpell> UsableSpells { get; }

        public virtual bool IsPowerClassActive
        {
            get { return _ActCtrl.LastValue.IsActive; }
            set
            {
                if (value != _ActCtrl.LastValue.IsActive)
                {
                    var _newAct = new Activation(this, value);
                    if (!_ActCtrl.WillAbortChange(_newAct))
                    {
                        _ActCtrl.DoPreValueChanged(_newAct);
                        _ActCtrl.DoValueChanged(_newAct);

                        // power level
                        _PowerLevel.IsActive = value;
                    }
                }
            }
        }

        #region IControlChange<Activation> Members

        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        public PowerClassInfo ToPowerClassInfo()
            => ToCasterClassInfo();

        // ICasterClass Members
        public CasterClassInfo ToCasterClassInfo()
            => new CasterClassInfo
            {
                ID = ID,
                Message = ClassName,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                OwnerID = OwnerID.ToString(),
                Key = Key,
                IsPowerClassActive = IsPowerClassActive,
                MagicType = MagicType,
                Alignment = Alignment.ToString(),
                EffectiveLevel = EffectiveLevel.ToVolatileValueInfo(),
                SpellDifficultyBase = SpellDifficultyBase.ToDeltableInfo(),
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };

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
    }
}