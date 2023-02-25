using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// When an Observe is successful, this is used to probe for details
    /// </summary>
    public class ObserveDetails : Observe
    {
        public ObserveDetails(CoreActor actor, ISensorHost viewer, Locator targetLoc, Locator observerLoc, double distance)
            : base(actor, viewer, targetLoc, observerLoc, distance)
        {
        }

        private static ObserveDetailsHandler _Static = new ObserveDetailsHandler();
        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }
}
