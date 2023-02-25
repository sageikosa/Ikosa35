using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Gloves : SlottedItemBase
    {
        public Gloves(string name, int maxStructure)
            : this(name, LeatherMaterial.Static, maxStructure)
        {
        }

        public Gloves(string name, Material itemMaterial, int maxStructure)
            : base(@"Gloves", ItemSlot.HandsSlot)
        {
            InitItem(itemMaterial, maxStructure);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxStructure)
        {
            Sizer.NaturalSize = Size.Miniature;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxStructure;
            // TODO: ... break DC of 25.
            BaseWeight = 0.25d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"gloves";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
