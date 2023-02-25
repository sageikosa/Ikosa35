using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Cloak : SlottedItemBase
    {
        public Cloak(string name, int maxStructure)
            : this(name, ClothMaterial.Static, maxStructure)
        {
        }

        public Cloak(string name, Material itemMaterial, int maxStructure)
            : base(@"Cloak", ItemSlot.ShouldersSlot)
        {
            InitItem(itemMaterial, maxStructure);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxStructure)
        {
            Sizer.NaturalSize = Size.Small;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxStructure;
            // TODO: ... break DC of 25.
            BaseWeight = 1d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"cloak";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
