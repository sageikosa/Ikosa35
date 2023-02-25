using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Interactions.Action
{
    [Serializable]
    public class PickUp : InteractData
    {
        public PickUp(CoreActor actor)
            : base(actor)
        {
        }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }

        private static PickUpHandler _Static = new PickUpHandler();

        /// <summary>Provides a list of object within melee range of a creature that can be picked up</summary>
        public static IEnumerable<ICoreObject> PickUpList(Creature critter)
        {

            // TODO: generalize so that Mage Hand and Telekinesis can also use
            // TODO: also, excluding other objects might be useful...(such as a repository when used for load aiming)
            if (critter != null)
            {
                var _rgn = critter.GetMeleeReachRegion();
                var _located = critter.GetLocated();
                if (_located != null)
                {
                    foreach (var _obj in from _loc in _located.Locator.MapContext.LocatorsInRegion(_rgn, _located.Locator.PlanarPresence)
                                         where _loc != _located.Locator && _loc.ICore is ICoreObject
                                         select _loc.ICore as ICoreObject)
                        yield return _obj;
                }
            }
            yield break;
        }
    }

    [Serializable]
    public class PickUpFeedback : InteractionFeedback
    {
        public PickUpFeedback(object source)
            : base(source)
        {
        }
    }
}
