using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions.Action
{
    [Serializable]
    public class ManipulateTouch : InteractData
    {
        public ManipulateTouch(CoreActor actor)
            : base(actor)
        {
        }

        private static ManipulateTouchHandler _Static = new ManipulateTouchHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();

        public static bool CanManipulateTouch(Creature critter, ICoreObject coreObject)
        {
            var _grabObjectStep = new ManipulateTouch(critter);
            var _interact = new Interaction(critter, coreObject, coreObject, _grabObjectStep);
            coreObject.HandleInteraction(_interact);

            return _interact.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value);
        }
    }
}
