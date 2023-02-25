using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FixedWheels : Mechanism
    {
        public FixedWheels(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            // TODO: auto-roll on slope...
            AddAdjunct(new MechanismAdjunctInjector(new ObjectGrabbedPivotAllowance(this, 1)));
            AddAdjunct(new MechanismAdjunctInjector(new WheelManeuverabilityCost(this, 1.1d, 1.5d, 2d, false)));
        }

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        protected override string ClassIconKey => nameof(FixedWheels);
    }
}
