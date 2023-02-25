using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// When action time is completed, all rechargeable slots are recharged
    /// </summary>
    [Serializable]
    public class RechargeSlots : SimpleActionBase
    {
        /// <summary>
        /// When action time is completed, all rechargeable slots are recharged
        /// </summary>
        public RechargeSlots(Creature critter, ISpontaneousCaster caster, ActionTime actionTime)
            : base(critter, actionTime, true, false, @"102")
        {
            _Caster = caster;
        }

        #region data
        private ISpontaneousCaster _Caster;
        #endregion

        public override bool IsMental => true;
        public override string Key => @"Timeline.RechargeSlots";
        public override string DisplayName(CoreActor actor) => @"Meditation";
        public ISpontaneousCaster Caster => _Caster;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Meditation", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep($@"Spell slots recharged for {_Caster.ClassName}");

        public override bool DoStep(CoreStep actualStep)
        {
            var _time = ((actualStep.Process as CoreActivity)?.Actor?.Setting as ITacticalMap)?.CurrentTime ?? 0;
            foreach (var _slot in from _slotSet in _Caster.AllSpellSlotSets
                                  from _level in _slotSet.SlotSet.AllLevels
                                  from _s in _level.Slots
                                  where !_s.IsCharged && _s.CanRecharge(_time)
                                  select _s)
            {
                _slot.RechargeSlot(_time);
            }
            return true;
        }
    }
}
