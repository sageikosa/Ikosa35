using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Can fasten to a hasp.  It's blocker does not allow the Hasp to be closed when the padlock is secured.</summary>
    [Serializable]
    public class PadLock : FastenerObject, ISecureLock, IAudibleOpenable
    {
        #region Construction
        /// <summary>Can fasten to a hasp.  It's blocker does not allow the Hasp to be closed when the padlock is secured.</summary>
        public PadLock(string name, Material material, int disableDifficulty, int pickDifficulty, IEnumerable<Guid> keys)
            : base(name, material, disableDifficulty, false)
        {
            _Keys = new Collection<Guid>(keys.ToArray());
            _PickDifficulty = new Deltable(pickDifficulty);
            _KUCtrl = new ChangeController<SecureState>(this, new SecureState());
        }
        #endregion

        #region state
        private Collection<Guid> _Keys;
        private Deltable _PickDifficulty;
        private ChangeController<SecureState> _KUCtrl;
        #endregion

        protected override bool OnWillAbort(OpenStatus openState)
            => base.OnWillAbort(openState);

        #region public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            // when !IsClosed ==> padlock is unlocked...
            if (!OpenState.IsClosed)
            {
                if (_budget.CanPerformRegular)
                {
                    if (FastenTarget == null)
                    {
                        // binds the unlocked padlock to the hasp (does not necessarily lock it)
                        yield return new FastenToHasp(this, @"202");
                    }
                    else
                    {
                        // unbinds the unlocked padlock from its hasp...
                        yield return new UnfastenFromHasp(this, @"201");
                    }
                }

                // if the lock has not been deactivated, this allows the padlock to close
                if (Activation.IsActive && _budget.CanPerformBrief)
                {
                    yield return new OpenCloseAction(this, this, @"101");
                }
            }

            if (OpenState.IsClosed && _budget.CanPerformTotal)
            {
                yield return new PickLockAction(this, @"102");
            }

            foreach (var _act in BaseMechanismActions(budget))
            {
                yield return _act;
            }

            yield break;
        }
        #endregion

        protected override IOpenable FastenTarget
            => Adjuncts.OfType<ObjectBound>().FirstOrDefault()?.Anchorage as IOpenable;

        #region IKeySlot Members

        public IEnumerable<Guid> Keys => _Keys.Select(_k => _k);
        public Deltable PickDifficulty => _PickDifficulty;

        public void SecureLock(CoreActor actor, object source, bool success)
        {
            if (Activation.IsActive && success && !OpenState.IsClosed)
            {
                this.CompleteOpenClose(this.StartOpenClose(actor, source, 0));
                success = success && (OpenState.Value == 0);
            }
            _KUCtrl.DoValueChanged(new SecureState() { Source = source, Securing = true, Success = success });
        }

        public void UnsecureLock(CoreActor actor, object source, bool success)
        {
            if (Activation.IsActive && success && OpenState.IsClosed)
            {
                this.CompleteOpenClose(this.StartOpenClose(actor, source, 1));
                success = success && (OpenState.Value == 1);
            }
            _KUCtrl.DoValueChanged(new SecureState() { Source = source, Securing = false, Success = success });
        }

        #endregion

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

        #region IControlChange<KeyUse> Members

        public void AddChangeMonitor(IMonitorChange<SecureState> monitor)
        {
            _KUCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<SecureState> monitor)
        {
            _KUCtrl.RemoveChangeMonitor(monitor);
        }

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

        protected override string ClassIconKey
            => nameof(PadLock);
    }
}
