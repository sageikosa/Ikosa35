using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ExtractNaturalItemHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is ExtractNaturalItemData _extract)
                && (workSet.Target is ISlottedItem _target)
                    && (workSet.Actor is Creature _critter))
            {
                // disconnect from it's source
                var _original = _target.CreaturePossessor;
                _target.ClearSlots();
                _extract.NaturalItemTrait.ExtractItem();

                // need to make sure object and critter have same ethereal state
                var _objEthereal = _target.PathHasActiveAdjunct<EtherealState>();
                if (_objEthereal)
                {
                    // make sure object's etherealness has continuity
                    EtherealEffect.EmbedEthereal(_target);
                }

                if (_critter.Body.ItemSlots[ItemSlot.HoldingSlot, true] is HoldingSlot _slot)
                {
                    if (_target is ItemBase _item)
                    {
                        // try to take possession of slotted item
                        if (_item.Possessor != _critter)
                        {
                            _item.Possessor = _critter;
                        }
                    }

                    // direct or holding wrapper
                    var _slotItem = HoldingSlot.GetHoldableItem(_target, _critter);

                    // see if the load can take it...
                    if ((_slotItem.Possessor == _critter) && _critter.ObjectLoad.CanAdd(_target))
                    {
                        // slot the item
                        _slotItem.SetItemSlot(_slot);
                        return;
                    }
                }

                // just drop it in the environment
                Drop.DoDrop(_original, _target, this, false);
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ExtractNaturalItemData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
