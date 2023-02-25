using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class CreatureStenchActivation : SimpleActionBase
    {
        public CreatureStenchActivation(CreatureStenchControl control, ActionTime cost, bool setActive)
            : base(control, cost, false, false, @"201")
        {
            _SetActive = setActive;
        }

        #region state
        private bool _SetActive;
        #endregion

        public CreatureStenchControl CreatureStenchControl => ActionSource as CreatureStenchControl;
        public override bool IsMental => true;
        public override string Key => $@"Stench.{CreatureStenchControl.ID}.{(_SetActive ? "ON" : "OFF")}";

        public override string DisplayName(CoreActor observer)
            => $@"{(_SetActive ? string.Empty : "De-")}Activating {CreatureStenchControl.Stench.PoisonProvider.Name} Stench";

        public override bool DoStep(CoreStep actualStep)
        {
            if (_SetActive)
            {
                CreatureStenchControl.EnableStench();
            }
            else
            {
                CreatureStenchControl.DisableStench();
            }
            return true;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Stinking up the place", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep($@"Stench {(_SetActive ? string.Empty : "De-")}Activated");
    }
}
