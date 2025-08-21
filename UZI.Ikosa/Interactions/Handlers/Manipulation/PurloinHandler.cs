using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PurloinHandler : ManipulationHandlerBase, IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Purloin _purloin)
            {
                if ((workSet.Target is ICoreObject _obj)
                    && (workSet.Actor is Creature _critter))
                {
                    // is target a slotted item in its designated slot?
                    if ((_obj is ISlottedItem _slotted)
                        && (_slotted.MainSlot != null)
                        // and also, is it impossible to unslot by item or slot or in the action time?
                        && ((_slotted.UnslottingTime == null)
                            || (_slotted is NaturalWeapon)
                            || !(_slotted.MainSlot?.AllowUnSlotAction ?? true)
                            || ((_slotted.UnslottingTime.ActionTimeType >= Contracts.TimeType.Regular)
                                && (_slotted.UnslottingTime > _purloin.ActionTime)
                                )
                            )
                        )
                    {
                        // cannot purloin
                        workSet.Feedback.Add(new UnderstoodFeedback(this));
                        return;
                    }
                    // need to make sure object and critter have same ethereal state
                    var _objEthereal = _obj.PathHasActiveAdjunct<EtherealState>();
                    if ((_objEthereal != _critter.PathHasActiveAdjunct<EtherealState>())
                        || !CanSubstantiallyInteract(_critter, _obj))
                    {
                        workSet.Feedback.Add(new InfoFeedback(this, new Info { Message = @"Unable to hold" }));
                    }
                    else
                    {
                        if (_objEthereal)
                        {
                            // make sure object's etherealness has continuity
                            EtherealEffect.EmbedEthereal(_obj);
                        }

                        // disconnect from it's source
                        var _connector = _obj.Adjuncts.OfType<ISlotPathed>().FirstOrDefault()?.SlottedConnector;
                        if (_connector != null)
                        {
                            var _original = _connector.CreaturePossessor;
                            _connector.ClearSlots();

                            // if sourced from DetachItem, see if there is an empty holding slot
                            // otherwise, see if the source is a holding slot
                            var _slot = (workSet.Source is DetachItem)
                                ? _critter.Body.ItemSlots[ItemSlot.HoldingSlot, true] as HoldingSlot
                                : workSet.Source as HoldingSlot;

                            if (_slot != null)
                            {
                                if (_obj is ItemBase _item)
                                {
                                    // try to take possession of slotted item
                                    if (_item.Possessor != _critter)
                                    {
                                        _item.Possessor = _critter;
                                    }
                                }

                                // direct or holding wrapper
                                var _slotItem = HoldingSlot.GetHoldableItem(_obj, _critter);

                                // see if the load can take it...
                                if ((_slotItem.Possessor == _critter) && _critter.ObjectLoad.CanAdd(_obj))
                                {
                                    // slot the item
                                    _slotItem.SetItemSlot(_slot);
                                    return;
                                }
                            }

                            // just drop it in the environment
                            Drop.DoDrop(_original, _obj, this, false);
                        }
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Purloin);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }

        #endregion
    }
}
