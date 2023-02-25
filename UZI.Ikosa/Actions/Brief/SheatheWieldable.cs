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
    public class SheatheWieldable : ActionBase
    {
        public SheatheWieldable(MountSlot slot, ActionTime timeCost, IEnumerable<OptionAimValue<ISlottedItem>> mountables, string orderKey)
            : base(slot, timeCost, true, false, orderKey)
        {
            _Mountables = mountables.ToList();
        }

        private List<OptionAimValue<ISlottedItem>> _Mountables;

        public MountSlot MountSlot => ActionSource as MountSlot;
        public override string Key => @"SheatheWieldable";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override string DisplayName(CoreActor actor)
            => $@"Sheathe into {MountSlot.SlotType} ({MountSlot.SubType})";

        #region public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _target = activity.Targets.OfType<OptionTarget>().FirstOrDefault();
            if ((_target != null) && (_target.Key == @"Item"))
            {
                if (_target.Option is OptionAimValue<ISlottedItem> _item)
                {
                    return ObservedActivityInfoFactory.CreateInfo(@"Sheathing Item", activity.Actor, observer, _item.Value);
                }
            }
            return ObservedActivityInfoFactory.CreateInfo(@"Sheathing Item", activity.Actor, observer);
        }
        #endregion

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _target = activity.Targets.OfType<OptionTarget>().FirstOrDefault();
            if ((_target != null) && (_target.Key == @"Item"))
            {
                if (_target.Option is OptionAimValue<ISlottedItem> _item)
                {
                    activity.EnqueueRegisterPreEmptively(Budget);

                    // unslot from holding slots
                    _item.Value.ClearSlots();

                    // mount in mounting slot (create wrapper if needed)
                    if (MountSlot.SlottedItem == null)
                    {
                        var _wrap = new MountWrapper(MountSlot.Creature, _item.Value as IWieldMountable, MountSlot.SlotType);
                        _wrap.SetItemSlot(MountSlot);
                    }
                    else
                    {
                        (MountSlot.SlottedItem as IMountWrapper).MountItem(_item.Value as IWieldMountable);
                    }

                    // feedback
                    var _info = GetInfoData.GetInfoFeedback(_item.Value, MountSlot.Creature);
                    if (_info != null)
                    {
                        var _step = activity.GetActivityResultNotifyStep(@"Sheathed");
                        _step.AppendFollowing(activity.GetNotifyStep(
                            new RefreshNotify(true, false, false, true, false)));
                        return _step;
                    }
                    else
                    {
                        var _step = activity.GetActivityResultNotifyStep(@"Sheathed");
                        _step.AppendFollowing(activity.GetNotifyStep(
                            new RefreshNotify(true, false, false, true, false)));
                        return _step;
                    }
                }
            }
            return null;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Item", @"Item to Sheathe", true, FixedRange.One, FixedRange.One, _Mountables);
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}