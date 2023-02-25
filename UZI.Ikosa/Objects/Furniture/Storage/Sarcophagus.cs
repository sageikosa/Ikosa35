using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Sarcophagus : HollowFurnishing
    {
        public Sarcophagus(string name, Material material)
            : base(name, material)
        {
        }

        public override HollowFurnishingLid CreateLid(Material material)
            => new SarcophagusLid(material)
            {
                Width = Width,
                Length = Length,
                Height = 0.1650,
                TareWeight = TareWeight / 2,
                MaxStructurePoints = MaxStructurePoints / 2
            };

        public override bool IsHardSnap(AnchorFace face)
            => true;

        protected override HollowFurnishing GetClone()
            => new Sarcophagus(Name, ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Sarcophagus);
    }

    [Serializable]
    public class SarcophagusLid : HollowFurnishingLid
    {
        public SarcophagusLid(Material material)
            : base(@"Sarcophagus Lid", material)
        {
        }

        public override object Clone()
            => new SarcophagusLid(ObjectMaterial);

        public override bool IsValidLid(HollowFurnishing hollowFurnishing)
            => hollowFurnishing is Sarcophagus;
    }
}
