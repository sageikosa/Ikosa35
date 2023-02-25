using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Bed : MidSurfaceFurnishing
    {
        public Bed(Items.Materials.Material material)
            : base(nameof(Bed), material)
        {
        }

        protected override MidSurfaceFurnishing GetClone()
            => new Bed(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Bed);
    }
}
