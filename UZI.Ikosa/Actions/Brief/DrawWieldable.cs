using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DrawWieldable : ActionBase
    {
        public DrawWieldable(MountSlot slot, ActionTime timeCost, string orderKey)
            : base(slot, timeCost, false, false, orderKey)
        {
        }

        public MountSlot MountSlot => ActionSource as MountSlot;
        public override string Key => @"DrawWieldable";
        public override bool IsMental => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
        {
            // if drawing as a free action, do not interrupt stacked activities
            if (TimeCost.ActionTimeType == TimeType.Free)
            {
                return false;
            }

            return base.WillClearStack(budget, activity);
        }

        public override string DisplayName(CoreActor actor)
            => $@"Draw {MountSlot.SlottedItem.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _slot = activity.Targets.OfType<OptionTarget>()
                .Select(_ot => _ot.Option).OfType<OptionAimValue<HoldingSlot>>()
                .Select(_hst => _hst.Value).FirstOrDefault();
            return ObservedActivityInfoFactory.CreateInfo(@"Drawing Item", activity.Actor, observer,
                _slot?.SlottedItem ?? MountSlot.SlottedItem);
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _wieldable = MountSlot.MountedItem;
            if (_wieldable != null)
            {
                // draw to slot(s)
                var _slots = activity.Targets.OfType<OptionTarget>()
                    .Select(_ot => _ot.Option).OfType<OptionAimValue<HoldingSlot>>()
                    .Select(_hst => _hst.Value).ToList();

                activity.EnqueueRegisterPreEmptively(Budget);

                // unmount from wrapper
                MountSlot.MountWrapper.UnmountItem();

                // wield in holding slot(s)
                if (_slots.Count == 2)
                {
                    _wieldable.SetItemSlot(_slots[0], _slots[1]);
                }
                else
                {
                    _wieldable.SetItemSlot(_slots[0]);
                }

                // feedback
                var _info = GetInfoData.GetInfoFeedback(_wieldable, MountSlot.Creature);
                if (_info != null)
                {
                    // TODO: _info for wieldable
                    var _step = activity.GetActivityResultNotifyStep(@"Wielding");
                    _step.AppendFollowing(activity.GetNotifyStep(
                        new RefreshNotify(true, false, false, true, false)));
                    return _step;
                }
                else
                {
                    var _step = activity.GetActivityResultNotifyStep(@"Wielding");
                    _step.AppendFollowing(activity.GetNotifyStep(
                        new RefreshNotify(true, false, false, true, false)));
                    return _step;
                }
            }
            return null;
        }
        #endregion

        private IEnumerable<OptionAimOption> TargetSlots()
            => from _hs in MountSlot.Creature.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
               where _hs.SlottedItem == null
               select new OptionAimValue<HoldingSlot>
               {
                   Key = _hs.ID.ToString(),
                   Name = $@"{_hs.SlotType} ({_hs.SubType})",
                   Value = _hs
               };

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // NOTE: can always put in two holding slots
            yield return new OptionAim(@"TargetSlot", @"Holding Slot", true, FixedRange.One, new FixedRange(2), TargetSlots());
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
