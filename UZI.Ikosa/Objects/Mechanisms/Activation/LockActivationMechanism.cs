using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class LockActivationMechanism : ActivationMechanism, ISecureLock
    {
        public LockActivationMechanism(string name, Material material, int disableDifficulty,
            int pickDifficulty, IEnumerable<Guid> keys, ActivationMechanismStyle activationMechanismStyle)
            : base(name, material, disableDifficulty, activationMechanismStyle)
        {
            _Keys = new HashSet<Guid>(keys.Distinct());
            _PickDifficulty = new Deltable(pickDifficulty);
            _KUCtrl = new ChangeController<SecureState>(this, new SecureState());
            ObjectSizer.NaturalSize = Size.Fine;
        }

        #region data
        private HashSet<Guid> _Keys;
        private Deltable _PickDifficulty;
        private ChangeController<SecureState> _KUCtrl;
        #endregion

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            if (Activation.IsActive)
            {
                // NOTE: use key will attempt to lock/unlock
                if (_budget?.CanPerformTotal ?? false)
                {
                    yield return new PickLockAction(this, @"102");
                }
            }

            foreach (var _act in BaseMechanismActions(budget))
            {
                yield return _act;
            }

            yield break;
        }

        public HashSet<Guid> KeySet => _Keys;

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

        /// <summary>set next state for the securing parameter</summary>
        private void SetNextState(object source, bool success, bool securing)
        {
            foreach (var _actObj in ActivatableObjects)
            {
                var _next = ActivationMechanismStyle == ActivationMechanismStyle.FlipFlop
                    ? !_actObj.Activation.IsActive
                    : securing;
                if (Activation.IsActive && success && (_actObj.Activation.IsActive != _next))
                {
                    _actObj.Activation = new Activation(this, _next);
                    success = success && (_actObj.Activation.IsActive == _next);
                }
            }
            _KUCtrl.DoValueChanged(new SecureState() { Source = source, Securing = securing, Success = success });
        }

        public void SecureLock(CoreActor actor, object source, bool success)
        {
            SetNextState(source, success, true);
        }

        public void UnsecureLock(CoreActor actor, object source, bool success)
        {
            SetNextState(source, success, false);
        }
        #endregion

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
    }
}
