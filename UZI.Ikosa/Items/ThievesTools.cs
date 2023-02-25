using System;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [ItemInfo(@"Thieve's Tools", @"Needed to overcome -2 Pick Lock Penalty", @"thieves_tools"), Serializable]
    public class ThievesTools : SlottedItemBase
    {
        public ThievesTools()
            : base(@"Thieves' Tools", ItemSlot.HoldingSlot)
        {
            InitItem(30, SteelMaterial.Static, 5, Size.Fine);
        }

        private void InitItem(decimal basePrice, Material itemMaterial, int maxHP, Size size)
        {
            Price.CorePrice = basePrice;
            Sizer.NaturalSize = size;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            BaseWeight = 1d;
        }

        protected override string ClassIconKey => @"thieves_tools";

        public override bool IsTransferrable => true;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
