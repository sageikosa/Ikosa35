using System;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Backpack", @"Holds items", @"backpack")
    ]
    public class BackPack : SlottedContainerItemBase
    {
        public BackPack()
            : this(@"Backpack", true)
        {
        }
        public BackPack(string name, bool opaque)
            : base(name, ItemSlot.BackSlot, new ContainerObject(@"storage", LeatherMaterial.Static, true, false), opaque)
        {
            BaseWeight = 2;
            Price.CorePrice = 2;
            MaxStructurePoints.BaseValue = 2;
            ItemSizer.NaturalSize = Size.Small;
            Container.MaxStructurePoints = 1;
            Container.MaximumLoadWeight = 50;
            Container.TareWeight = 0;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override string ClassIconKey => @"backpack";
        public override bool IsTransferrable => true;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
