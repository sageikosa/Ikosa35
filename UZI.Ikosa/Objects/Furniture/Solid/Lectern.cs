using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Lectern : SolidFurnishing
    {
        public Lectern(Items.Materials.Material material)
            : base(nameof(Lectern), material)
        {
        }

        protected override SolidFurnishing GetClone()
            => new Lectern(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Lectern);
    }
}
