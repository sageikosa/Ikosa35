using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Ring : SlottedItemBase
    {
        public Ring(string name, Material itemMaterial, int maxStructure)
            : base(@"Ring", ItemSlot.RingSlot)
        {
            InitItem(itemMaterial, maxStructure);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxStructure)
        {
            Sizer.NaturalSize = Size.Fine;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxStructure;
            // TODO: ... break DC of 25.
            BaseWeight = 0.05d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey 
            => @"ring";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
