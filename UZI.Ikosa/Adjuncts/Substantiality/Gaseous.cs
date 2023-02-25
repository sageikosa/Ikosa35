using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Gaseous : Adjunct, IInteractHandler
    {
        public Gaseous(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject).AddIInteractHandler(this);
            // TODO: Gaseous movement (fly, no run); flow under/around (non-airtight) doors; affected by gusts/winds
            // TODO: DR 10/magic
            // TODO: no armor/no natural-armor (except force-based armor); size/deflect/dexterity/force still apply
            // TODO: no need to breathe; immune to stench, poison-gas
            // TODO: no physical attacks
            // TODO: no verbal/somatic/material/focus component use
            // TODO: suppress supernatural abilities (except gaseous form)
            // TODO: difficulty=15 to spot; hiding in misty/smokey area spot difficulty+20
            // TODO: dissipate held touch attacks
            // TODO: swap body with gas/material body
        }

        protected override void OnDeactivate(object source)
        {
            // TODO: Gaseous movement
            (Anchor as CoreObject).RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Gaseous(Source);

        // IInteractHandler
        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            // TODO: other damage/interactions...
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;
    }
}
