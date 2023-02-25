using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Altar : SolidFurnishing
    {
        public Altar(Items.Materials.Material material)
            : base(nameof(Altar), material)
        {
        }

        protected override SolidFurnishing GetClone()
            => new Altar(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Altar);
    }
}
