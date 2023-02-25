using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    public class Statue : SolidFurnishing
    {
        public Statue(Items.Materials.Material material)
            : base(nameof(Lectern), material)
        {
        }

        protected override SolidFurnishing GetClone()
            => new Statue(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Statue);
    }
}
