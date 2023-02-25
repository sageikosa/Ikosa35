using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class RetrieveHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Retrieve _retrieve)
            {
                if ((workSet.Target is ICoreObject _obj)
                    && (workSet.Actor is Creature _critter))
                {
                    var _objEthereal = _obj.PathHasActiveAdjunct<EtherealState>();
                    if (_objEthereal)
                    {
                        // make sure object's etherealness has continuity
                        EtherealEffect.EmbedEthereal(_obj);
                    }

                    if (_obj is ItemBase _item)
                    {
                        // try to take possession of slotted item
                        if (_item.Possessor != _critter)
                            _item.Possessor = _critter;
                    }
                    var _holdable = HoldingSlot.GetHoldableItem(_obj, _critter);

                    // see if the load can take it...
                    if ((_holdable.Possessor == _critter) && _critter.ObjectLoad.CanAdd(_obj))
                    {
                        // get item slot
                        var _slot = _critter.Body.ItemSlots[ItemSlot.HoldingSlot, true];
                        if (_slot != null)
                        {
                            // must remove from container before anything else
                            // NOTE: this can destroy a trove...
                            _retrieve.Repository.Remove(_obj);

                            // slot the item
                            _holdable.SetItemSlot(_slot);

                            if (_holdable.MainSlot == _slot)
                            {
                                // slotted!
                                workSet.Feedback.Add(new ValueFeedback<bool>(this, true));
                                return;
                            }
                            else
                            {
                                // didn't slot, drop instead
                                Drop.DoDropEject(_critter, _obj);
                                workSet.Feedback.Add(new InfoFeedback(this, new Info { Message = @"Unable to hold" }));
                            }
                            return;
                        }
                        workSet.Feedback.Add(new InfoFeedback(this, new Info { Message = @"No available holding slot" }));
                        return;
                    }
                    workSet.Feedback.Add(new InfoFeedback(this, new Info { Message = @"Couldn't take possession, or too heavy" }));
                    return;
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Retrieve);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }

        #endregion
    }
}
