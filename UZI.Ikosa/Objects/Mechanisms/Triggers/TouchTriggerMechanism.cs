using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TouchTriggerMechanism : TriggerMechanism
    {
        public TouchTriggerMechanism(string name, Material material, int disableDifficulty,
            PostTriggerState postState)
            : base(name, material, disableDifficulty, postState)
        {
            // --- true touch ---
            // Grab/grasp: GraspData (interactData)
            // Probe:
            // Touch...
            // DeliverHeldCharge...
            // AttackData:

            // ApplyItem:

            // --- inventory/slotting ---
            // Pickup: PickUp (interactData): not MechMount

            // --- furnishing ---
            // GrabObject: on adjunct added: not MechMount

            // --- searching ---
            // Search: SearchData (interactData)...fail -> trigger
            // TODO: non-touching-search

            // ISecureLock: ChangeController<SecureState>...separate trigger?!?
        }

        protected override string ClassIconKey => nameof(TouchTriggerMechanism);
    }
}
