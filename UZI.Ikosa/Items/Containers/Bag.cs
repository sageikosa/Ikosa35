using System;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Bag", @"Holds items", @"bag")
    ]
    public class Bag : ContainerItemBase
    {
        public Bag()
            : this(@"Bag", true)
        {
        }
        public Bag(string name, bool opaque)
            : base(name, new ContainerObject(@"storage", LeatherMaterial.Static, true, false), opaque)
        {
            BaseWeight = 2;
            Price.CorePrice = 2;
            MaxStructurePoints.BaseValue = 2;
            Container.MaximumLoadWeight = 25;
            Container.MaxStructurePoints = 1;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override string ClassIconKey { get { return @"bag"; } }
    }
}
