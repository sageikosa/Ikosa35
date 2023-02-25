using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SwitchActivationMechanism : ActivationMechanism, IAudibleOpenable
    {
        public SwitchActivationMechanism(string name, Material material, int disableDifficulty,
            ActivationMechanismStyle activationMechanismStyle)
            : base(name, material, disableDifficulty, activationMechanismStyle)
        {
            _OSCtrl = new ChangeController<OpenStatus>(this, this.GetOpenStatus(null, this, 1));
            ObjectSizer.NaturalSize = Size.Miniature;
        }

        #region data
        private ChangeController<OpenStatus> _OSCtrl;
        #endregion

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OSCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OSCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        public double OpenWeight { get => Weight; set => DoPropertyChanged(nameof(OpenWeight)); }

        public bool CanChangeOpenState(OpenStatus testValue)
            => !_OSCtrl.WillAbortChange(testValue);

        public OpenStatus OpenState
        {
            get => _OSCtrl.LastValue;
            set
            {
                if ((_OSCtrl.LastValue.Source != value.Source) || (_OSCtrl.LastValue.Value != value.Value))
                {
                    // Ensure it is allowed (something may prevent switch from being thrown)
                    if (_OSCtrl.WillAbortChange(value))
                    {
                        return;
                    }

                    // do pre-value changed
                    _OSCtrl.DoPreValueChanged(value);

                    // do value changed
                    _OSCtrl.DoValueChanged(value);
                    OnSwitch();

                    DoPropertyChanged(nameof(OpenState));
                }
            }
        }

        #region IAudibleOpenable Members
        protected string GetMaterialString()
            => $@"{ObjectMaterial.SoundQuality}";

        public SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opened", 
                (0, @"click"),
                (8, $@"mechanism")),
                10, 90, serialState);

        public SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed", 
                (0, @"click"),
                (8, $@"mechanism")),
                10, 90, serialState);

        public SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"blocked", 
                (0, @"rattling"),
                (10, $@"{GetMaterialString()} rattling")),
                8, 90, serialState);
        #endregion

        protected void OnSwitch()
        {
            foreach (var _actObj in ActivatableObjects)
            {
                var _next = ActivationMechanismStyle == ActivationMechanismStyle.FlipFlop
                    ? !_actObj.Activation.IsActive  // flip-flop switch
                    : OpenState.IsClosed;          // currently closed circuit == active object
                if (Activation.IsActive && (_actObj.Activation.IsActive != _next))
                {
                    _actObj.Activation = new Activation(this, _next);
                }
            }
        }

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            if (Activation.IsActive && _budget.CanPerformBrief)
            {
                yield return new UseSwitch(this, this, @"101");
            }

            foreach (var _act in BaseMechanismActions(budget))
                yield return _act;
            yield break;
        }

        // TODO: if not a flip-flop, prevent activation if de-activated
    }
}
