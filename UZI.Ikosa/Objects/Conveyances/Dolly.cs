using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Dolly : Conveyance
    {
        public Dolly(string name, Material objectMaterial)
            : base(name, objectMaterial)
        {
            (new RollingCasters(@"Rolling Casters", SteelMaterial.Static, 20)).BindToObject(this);
        }

        // NOTE: follow

        public override IGeometricSize GeometricSize => new GeometricSize(1,1,1);

        protected override string ClassIconKey => nameof(Dolly);

        public override object Clone()
        {
            var _clone = new Dolly(Name, ObjectMaterial);
            _clone.CopyFrom(this);
            return _clone;
        }

        public override IEnumerable<CoreAction> GetGrabbedActions(CoreActionBudget budget)
        {
            // TODO: ... steer
            // TODO: ... dump/flip
            yield break;
        }
    }
}
