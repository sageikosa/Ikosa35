using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SteerableWheels : Mechanism
    {
        public SteerableWheels(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            // TODO: change heading during move...
            // TODO: auto-roll on slope...
            AddAdjunct(new MechanismAdjunctInjector(new ObjectGrabbedPivotAllowance(this, 2)));
            AddAdjunct(new MechanismAdjunctInjector(new WheelManeuverabilityCost(this, 1.1d, 1.25d, 2d, false)));
        }

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        protected override string ClassIconKey => nameof(SteerableWheels);
    }
}
