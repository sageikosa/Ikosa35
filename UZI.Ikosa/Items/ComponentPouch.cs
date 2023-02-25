using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable,
    ItemInfo(@"Component Pouch", @"Holds standard material components for spells", @"component_pouch")]
    public class ComponentPouch : SlottedItemBase
    {
        public ComponentPouch()
            : base(@"Component Pouch", ItemSlot.Pouch)
        {
            InitItem(5, LeatherMaterial.Static, 2, Size.Fine);
        }

        private void InitItem(decimal basePrice, Material itemMaterial, int maxHP, Size size)
        {
            Price.CorePrice = basePrice;
            Sizer.NaturalSize = size;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            BaseWeight = 0.25d;
        }

        protected override string ClassIconKey => @"component_pouch";

        public override bool IsTransferrable
            => true;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
