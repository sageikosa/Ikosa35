using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class RollingCasters : Mechanism
    {
        public RollingCasters(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            // TODO: auto-roll on slope...
            AddAdjunct(new MechanismAdjunctInjector(new ObjectGrabbedPivotAllowance(this, 4)));
            AddAdjunct(new MechanismAdjunctInjector(new ObjectGrabbedManeuverabilityCost(this, 1.1, typeof(LandMovement))));
        }

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        protected override string ClassIconKey => nameof(RollingCasters);
    }
}
