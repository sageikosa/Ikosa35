using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Stool : SupportedSurface
    {
        public Stool(Items.Materials.Material material)
            : base(nameof(Stool), material)
        {
        }

        protected override SupportedSurface GetClone()
            => new Stool(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Stool);
    }
}
