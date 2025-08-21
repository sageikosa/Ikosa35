using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Unsecured mechanism to unlock a lock.
    /// </summary>
    [Serializable]
    public class LockKnob : Mechanism, ILockMechanism, IAudibleOpenable
    {
        #region ctor()
        public LockKnob(string name, Material material, int disableDifficulty)
            : base(name, material, disableDifficulty)
        {
            _OpenState = this.GetOpenStatus(null, this, 1);
            _OCtrl = new ChangeController<OpenStatus>(this, _OpenState);
            ObjectSizer.NaturalSize = Size.Miniature;

            Activation = new Activation(this, true);
        }
        #endregion

        #region Data
        private OpenStatus _OpenState;
        private ChangeController<OpenStatus> _OCtrl;
        #endregion

        #region public LockGroup LockGroup { get; set; }
        public LockGroup LockGroup
        {
            get => (from _lm in Adjuncts.OfType<LockMechanism>()
                    select _lm.LockGroup).FirstOrDefault();
            set
            {
                // remove old lock mechanism (lock group will evaporate if no more mechanisms)
                Adjuncts.OfType<LockMechanism>().FirstOrDefault()?.Eject();

                // add new lock mechanism
                if (value != null)
                {
                    AddAdjunct(new LockMechanism(value));
                }
            }
        }
        #endregion

        #region public IOpenable Openable { get; set; }
        /// <summary>
        /// Creates a new LockGroup for an IOpenable and connects to this LockKnob
        /// </summary>
        public IOpenable Openable
        {
            get => LockGroup?.Target?.Openable;
            set
            {
                if (value != Openable)
                {
                    // clear old lock group
                    LockGroup = null;
                    if (value != null)
                    {
                        // make a new lock group
                        var _group = new LockGroup(@"Lock", false, true);
                        value.AddAdjunct(new LockTarget(_group));
                        LockGroup = _group;
                    }
                }
            }
        }
        #endregion

        #region IOpenable Members
        protected virtual void OnOpen()
        {
            var _lock = LockGroup;
            if (_lock != null)
            {
                // an open lock knob unlocks the lock
                if (Activation.IsActive && _lock.IsLocked)
                {
                    _lock.IsLocked = false;
                }
            }
        }
        protected virtual void OnClose()
        {
            var _lock = LockGroup;
            if (_lock != null)
            {
                // a closed lock knob locks the lock
                if (Activation.IsActive && !_lock.IsLocked)
                {
                    _lock.IsLocked = true;
                }
            }
        }

        public bool CanChangeOpenState(OpenStatus testValue)
            => !_OCtrl.WillAbortChange(testValue);

        public OpenStatus OpenState
        {
            get => _OpenState;
            set
            {
                if (!OnWillAbort(value))
                {
                    var _oldClosed = _OpenState.IsClosed;

                    // change
                    _OCtrl.DoPreValueChanged(value);
                    _OpenState = value;
                    _OCtrl.DoValueChanged(value);
                    DoPropertyChanged(nameof(OpenState));

                    // perform fastener behavior
                    if (_oldClosed && !value.IsClosed)
                    {
                        OnOpen();
                    }

                    if (!_oldClosed && value.IsClosed)
                    {
                        OnClose();
                    }
                }
            }
        }

        public double OpenWeight { get => Weight; set => DoPropertyChanged(nameof(OpenWeight)); }
        #endregion

        #region IAudibleOpenable Members
        protected string GetMaterialString()
            => $@"{ObjectMaterial.SoundQuality}";

        public SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opened", 
                (0, @"click"),
                (5, $@"mechanism"),
                (10, $@"lock")),
                10, 90, serialState);

        public SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed", 
                (0, @"click"),
                (5, $@"mechanism"),
                (10, $@"lock")),
                10, 90, serialState);

        public SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"blocked", 
                (0, @"rattling"),
                (10, $@"{GetMaterialString()} rattling")),
                8, 90, serialState);
        #endregion

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        #region IControlChange<OpenStatus> Members
        /// <summary>override to check for more than just the openStatus change controller</summary>
        protected virtual bool OnWillAbort(OpenStatus openState)
        {
            return _OCtrl.WillAbortChange(openState);
        }

        public void AddChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        protected override string ClassIconKey
            => nameof(LockKnob);
    }
}
