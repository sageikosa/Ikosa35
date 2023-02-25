using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PickUpHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is PickUp)
            {
                var _obj = workSet.Target as ICoreObject;
                var _slot = workSet.Source as HoldingSlot;
                var _critter = _slot.Creature;
                if (_obj != _critter)
                {
                    // need to make sure object and critter have same ethereal state
                    var _objEthereal = _obj.PathHasActiveAdjunct<EtherealState>();
                    if (_objEthereal)
                    {
                        // make sure object's etherealness has continuity
                        EtherealEffect.EmbedEthereal(_obj);
                    }

                    var _located = _obj.GetDirectLocated();
                    if (_obj is ItemBase _item)
                    {
                        // try to take possession of slotted item
                        if (_item.Possessor != _critter)
                            _item.Possessor = _critter;
                    }
                    var _slotItem = HoldingSlot.GetHoldableItem(_obj, _critter);

                    // see if the load can take it...
                    if ((_slotItem?.Possessor == _critter) && _critter.ObjectLoad.CanAdd(_obj))
                    {
                        // if it was contained, find container to remove from
                        _obj.ContainedWithin()?.Container.Remove(_obj);

                        // slot the item
                        _slotItem.SetItemSlot(_slot);
                        if (_slotItem.MainSlot == _slot)
                        {
                            // if it was directly located, remove the locator
                            _located?.Locator.MapContext.Remove(_located?.Locator);
                            workSet.Feedback.Add(new PickUpFeedback(this));
                        }
                        else
                        {
                            // didn't slot, drop instead
                            Drop.DoDropEject(_critter, _obj);
                            workSet.Feedback.Add(new InfoFeedback(this, new Info { Message = @"Unable to hold" }));
                        }
                    }
                    else
                    {
                        // TODO: too heavy?
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(PickUp);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }

        #endregion
    }
}
