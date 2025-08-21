using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class CreatureGetInfoDataHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is GetInfoData)
            {
                if (workSet.Target is Creature _critter)
                {
                    var _observer = workSet.Actor as Creature;
                    var _info = _critter.GetInfo(_observer, true);
                    if (_info is CreatureObjectInfo _critterInfo)
                    {
                        // amend critterInfo with connected objects of which creature is aware
                        var _items = new List<ItemSlotInfo>();
                        foreach (var _slot in _critter.Body.ItemSlots.AllSlots)
                        {
                            // make sure observer is aware of each slotted item to collect
                            if (_slot.SlottedItem != null)
                            {
                                if (_observer.Awarenesses[_slot.SlottedItem.BaseObject.ID] == AwarenessLevel.Aware)
                                {
                                    // get slot info and object info
                                    if (_slot is MountSlot)
                                    {
                                        _items.Add((_slot as MountSlot).ToMountSlotInfo(_observer));
                                    }
                                    else
                                    {
                                        _items.Add(_slot.ToItemSlotInfo(_observer));
                                    }
                                }
                            }
                        }

                        // put gathered item infos into the critter info
                        _critterInfo.ItemSlots = _items.ToArray();
                    }

                    // provide feedback
                    var _infoBack = new InfoFeedback(this, _info);
                    workSet.Feedback.Add(_infoBack);
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetInfoData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion
    }
}
