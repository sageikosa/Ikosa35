using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Chair : MidSurfaceFurnishing
    {
        public Chair(Items.Materials.Material material)
            : base(nameof(Chair), material)
        {
        }

        protected override MidSurfaceFurnishing GetClone()
            => new Chair(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Chair);
    }
}
