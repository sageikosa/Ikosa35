using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class WheelBarrow : Conveyance
    {
        public WheelBarrow(string name, Material objectMaterial)
            : base(name, objectMaterial)
        {
            (new SteerableWheels(@"Steerable Wheels", SteelMaterial.Static, 20)).BindToObject(this);
        }

        // NOTE: must be behind to push (or else risk dumping and move at higher cost)
        // NOTE: follow movement...(for locator directly behind at start of movement)
        // NOTE: balance, strength, disable device checks to avoid accidental dumping

        public override IGeometricSize GeometricSize => new GeometricSize(1, 1, 1);

        protected override string ClassIconKey => nameof(WheelBarrow);

        public override object Clone()
        {
            var _clone = new WheelBarrow(Name, ObjectMaterial);
            _clone.CopyFrom(this);
            return _clone;
        }

        public override IEnumerable<CoreAction> GetGrabbedActions(CoreActionBudget budget)
        {
            // TODO: ... steer (change heading independent of movement)
            // TODO: ... dump/flip
            yield break;
        }
    }
}
