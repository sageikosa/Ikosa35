using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Keyhole : Mechanism, ISecureLock, ILockMechanism
    {
        #region ctor()
        public Keyhole(string name, Material material, int disableDifficulty, int pickDifficulty,
            IEnumerable<Guid> keys)
            : base(name, material, disableDifficulty)
        {
            _Keys = new Collection<Guid>(keys.ToList());
            _PickDifficulty = new Deltable(pickDifficulty);
            _KUCtrl = new ChangeController<SecureState>(this, new SecureState());
            ObjectSizer.NaturalSize = Size.Fine;

            Activation = new Activation(this, true);
        }
        #endregion

        #region Data
        private Collection<Guid> _Keys;
        private Deltable _PickDifficulty;
        private ChangeController<SecureState> _KUCtrl;
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
        /// Creates a new LockGroup for an IOpenable and connects to this Keyhole
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

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            if (LockGroup != null)
            {
                if ((budget as LocalActionBudget)?.CanPerformTotal ?? false)
                {
                    yield return new PickLockAction(this, @"102");
                }
            }

            // base mechanism actions
            foreach (var _act in BaseMechanismActions(budget))
            {
                yield return _act;
            }

            yield break;
        }

        public Collection<Guid> KeyCollection => _Keys;

        public void AddKey(Guid key)
        {
            if (!_Keys.Contains(key))
            {
                _Keys.Add(key);
            }

            DoPropertyChanged(nameof(Keys));
        }

        public void RemoveKey(Guid key)
        {
            if (_Keys.Contains(key))
            {
                _Keys.Remove(key);
            }

            DoPropertyChanged(nameof(Keys));
        }

        public IEnumerable<KeyValuePair<Guid, string>> NamedKeys
        {
            get
            {
                if (this.GetLocated().Locator.Map is LocalMap _map)
                {
                    foreach (var _k in Keys)
                    {
                        if (_map.NamedKeyGuids.ContainsKey(_k))
                        {
                            yield return new KeyValuePair<Guid, string>(_k, _map.NamedKeyGuids[_k]);
                        }
                    }
                }

                yield break;
            }
        }

        #region ISecureLock Members
        public IEnumerable<Guid> Keys
            => _Keys.Select(_k => _k);

        public Deltable PickDifficulty => _PickDifficulty;

        public void SecureLock(CoreActor actor, object source, bool success)
        {
            var _lock = LockGroup;
            if (_lock != null)
            {
                if (Activation.IsActive && success && !_lock.IsLocked)
                {
                    _lock.IsLocked = true;
                    success = success && _lock.IsLocked;
                }
                _KUCtrl.DoValueChanged(new SecureState() { Source = source, Securing = true, Success = success });
            }
        }

        public void UnsecureLock(CoreActor actor, object source, bool success)
        {
            var _lock = LockGroup;
            if (_lock != null)
            {
                if (Activation.IsActive && success && _lock.IsLocked)
                {
                    _lock.IsLocked = false;
                    success = success && !_lock.IsLocked;
                }
                _KUCtrl.DoValueChanged(new SecureState() { Source = source, Securing = false, Success = success });
            }
        }
        #endregion

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        #region IControlChange<SecureState> Members

        public void AddChangeMonitor(IMonitorChange<SecureState> monitor)
        {
            _KUCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<SecureState> monitor)
        {
            _KUCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        protected override string ClassIconKey
            => nameof(Keyhole);
    }
}
