using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class MountSlot : ItemSlot, IActionProvider
    {
        public MountSlot(object source, string mountType, string subType, bool magicCapable)
            : base(source, mountType, subType, magicCapable, false)
        {
        }

        public IMountWrapper MountWrapper => SlottedItem as IMountWrapper;
        public IWieldMountable MountedItem => MountWrapper?.MountedObject;

        #region private IEnumerable<OptionAimValue<ISlottedItem>> Mountables()
        /// <summary>OptionAimValues for SheatheWieldable</summary>
        public static IEnumerable<OptionAimValue<ISlottedItem>> Mountables(MountSlot slot)
        {
            // mountable options for this MountSlot
            foreach (var _opt in from _slottedItem in
                                     (from _hs in slot.Creature.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                                      let _wld = _hs.SlottedItem as IWieldMountable
                                      where (_wld != null) && _wld.SlotTypes.Contains(slot.SlotType)
                                      select _hs.SlottedItem).Distinct()
                                 let _info = GetInfoData.GetInfoFeedback(_slottedItem, slot.Creature)
                                 select new OptionAimValue<ISlottedItem>
                                 {
                                     Key = _slottedItem.ID.ToString(),
                                     Name = (_info != null) ? _info.Message : _slottedItem.Name,
                                     Value = _slottedItem
                                 })
            {
                yield return _opt;
            }

            yield break;
        }
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                if (_budget.Actor is Creature _critter)
                {
                    if (SlottedItem is IMountWrapper)
                    {
                        var _wrap = SlottedItem as IMountWrapper;
                        if (_wrap.MountedObject != null)
                        {
                            // slotted item is a wrapper, we wield directly from it
                            if (_critter.Feats.Contains(typeof(QuickDrawFeat))
                                && (SlotType != ItemSlot.BackShieldMount))
                            {
                                // quick draw as a free action (but not shields)
                                yield return new DrawWieldable(this, new ActionTime(TimeType.Free), @"101");
                            }
                            else
                            {
                                if ((_critter.BaseAttack.EffectiveValue >= 1)
                                   && (_budget.TopActivity?.Action is StartMove))
                                {
                                    // quick draw with BAB+1, and part of a regular move (including shields)
                                    yield return new DrawWieldable(this, new ActionTime(TimeType.Free), @"101");
                                }
                                else
                                {
                                    // otherwise, need brief to draw a wieldable item
                                    if (_budget.CanPerformBrief)
                                    {
                                        yield return new DrawWieldable(this, new ActionTime(TimeType.Brief), @"101");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_budget.CanPerformBrief)
                            {
                                yield return new SheatheWieldable(this, new ActionTime(TimeType.Brief), _wrap.GetMountables(), @"102");
                            }
                        }
                    }
                    else if (SlottedItem == null)
                    {
                        if (_budget.CanPerformBrief)
                        {
                            yield return new SheatheWieldable(this, new ActionTime(TimeType.Brief), Mountables(this), @"102");
                        }
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return ToMountSlotInfo();
        }

        #endregion

        public MountSlotInfo ToMountSlotInfo(Creature observer)
        {
            var _info = ToInfo<MountSlotInfo>(observer);
            if ((MountedItem != null) && (MountedItem is CoreObject))
            {
                _info.MountedItem = GetInfoData.GetInfoFeedback(MountedItem as CoreObject, observer) as ObjectInfo;
            }
            else
            {
                _info.MountedItem = null;
            }

            if ((MountWrapper != null) && (MountWrapper is CoreObject))
            {
                _info.MountWrapper = GetInfoData.GetInfoFeedback(MountWrapper as CoreObject, observer) as ObjectInfo;
            }
            else
            {
                _info.MountWrapper = null;
            }

            return _info;
        }

        public MountSlotInfo ToMountSlotInfo()
        {
            return ToMountSlotInfo(Creature);
        }
    }
}
