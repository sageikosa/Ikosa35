using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Cart : Conveyance
    {
        public Cart(string name, Material objectMaterial)
            : base(name, objectMaterial)
        {
            (new SteerableWheels(@"Steerable Wheels", SteelMaterial.Static, 20)).BindToObject(this);
        }

        public override IGeometricSize GeometricSize => new GeometricSize(1, 1, 1);

        protected override string ClassIconKey => nameof(Cart);

        public override object Clone()
        {
            var _clone = new Cart(Name, ObjectMaterial);
            _clone.CopyFrom(this);
            return _clone;
        }

        public override IEnumerable<CoreAction> GetGrabbedActions(CoreActionBudget budget)
        {
            // TODO: ... dump/flip
            yield break;
        }
    }
}
