using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public abstract class ManipulationHandlerBase
    {
        // test same plane
        protected bool CanSubstantiallyInteract(ICoreObject actor, ICoreObject target)
        {
            // excluded force, so check gaseous next (nothing overcomes that...)
            if (!actor.PathHasActiveAdjunct<Gaseous>() && !target.PathHasActiveAdjunct<Gaseous>())
            {
                // neither is gaseous, so check incorporeal
                if (actor.PathHasActiveAdjunct<Incorporeal>() || target.PathHasActiveAdjunct<Incorporeal>())
                {
                    // at least one is incorporeal
                    if (target.PathHasActiveAdjunct<GhostTouchProtector>() || target.PathHasActiveAdjunct<GhostTouchWeapon>())
                    {
                        // at least one has ghost touch
                        return true;
                    }
                }
                else
                {
                    // neither gaseous, neither incorporeal
                    return true;
                }
            }

            // gaseous or incorporeal with no ghost touch
            return false;
        }
    }
}
