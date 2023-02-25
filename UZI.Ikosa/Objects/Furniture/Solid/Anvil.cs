using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Anvil : SolidFurnishing
    {
        public Anvil(Items.Materials.Material material)
            : base(nameof(Anvil), material)
        {
        }

        protected override SolidFurnishing GetClone()
            => new Anvil(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Anvil);
    }
}
