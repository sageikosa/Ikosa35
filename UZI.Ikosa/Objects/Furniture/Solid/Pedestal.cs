using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Pedestal : SolidFurnishing
    {
        public Pedestal(Items.Materials.Material material)
            : base(nameof(Pedestal), material)
        {
        }

        protected override SolidFurnishing GetClone()
            => new Pedestal(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Pedestal);
    }
}
