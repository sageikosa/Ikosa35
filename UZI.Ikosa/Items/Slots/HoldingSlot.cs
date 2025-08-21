using Uzi.Core.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class HoldingSlot : ItemSlot, IActionProvider
    {
        public HoldingSlot(object source, string subType, bool magicCapable)
            : base(source, ItemSlot.HoldingSlot, subType, magicCapable, false)
        {
        }

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if ((SlottedItem != null) && (!(SlottedItem is NaturalWeapon)))
            {
                // free actions
                yield return new DropHeldObject(this, @"200");
                if (SlottedItem.AllSlots.Count() > 1)
                {
                    yield return new ReleaseOneHand(this, @"202");
                }

                if (_budget.CanPerformRegular)
                {
                    if (SlottedItem is IMeleeWeapon)
                    {
                        // probe
                        var _wpn = SlottedItem as IMeleeWeapon;
                        if ((_wpn is IMultiWeaponHead _multi) && _multi.IsDualWielding)
                        {
                            yield return new Probe(_wpn.MainHead, new ActionTime(TimeType.Regular), $@"901");
                            yield return new Probe(_multi.OffHandHead, new ActionTime(TimeType.Regular), $@"902");
                        }
                        else
                        {
                            yield return new Probe(_wpn.MainHead, new ActionTime(TimeType.Regular), $@"901");
                        }
                    }
                    else
                    {
                        // TODO: non-melee weapon probe
                    }
                }

                // holding a holding wrapper, containing a slotted item...
                if (((SlottedItem as HoldingWrapper)?.BaseObject is ISlottedItem)
                    && (_budget.Actor is Creature))
                {
                    var _wrapper = (SlottedItem as HoldingWrapper);
                    var _slottedItem = _wrapper.BaseObject as ISlottedItem;
                    var _sx = 1;
                    foreach (var _avail in (_budget.Actor as Creature).Body.ItemSlots.AvailableSlots(_slottedItem))
                    {
                        // if the action is in budget, yield it...
                        if (_budget.HasTime(_slottedItem.SlottingTime))
                        {
                            yield return new DonSlottedItem(this, _wrapper, _avail, $@"3{_sx:0#}");
                        }

                        _sx++;
                    }
                }
            }
            else
            {
                if (_budget.CanPerformBrief)
                {
                    yield return new PickUpObject(this, @"401");
                }
                if (_budget.CanPerformRegular)
                {
                    // grasp
                    yield return new Grasp(this, new ActionTime(TimeType.Regular), @"901");
                }
                foreach (var _item in (from _slot in Creature.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                                       let _si = _slot.SlottedItem
                                       where (_si != null) && (_si.SecondarySlot == null)
                                       select _si))
                {
                    // slot item into secondary slot also...
                    yield return new HoldSecondSlot(this, _item, @"201");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToItemSlotInfo();

        #endregion

        public override ItemSlot Clone(object source)
            => new HoldingSlot(source, SubType, MagicalSlot);

        public static ISlottedItem GetHoldableItem(ICoreObject objectToHold, Creature critter)
        {
            var _slotItem = objectToHold as ISlottedItem;
            if ((_slotItem == null) || !_slotItem.SlotType.Equals(ItemSlot.HoldingSlot, StringComparison.OrdinalIgnoreCase))
            {
                // need to wrap to hold it
                _slotItem = new HoldingWrapper(critter, objectToHold);
            }
            return _slotItem;
        }
    }
}
