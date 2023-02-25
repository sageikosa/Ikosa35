using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Boots : SlottedItemBase
    {
        public Boots(string name, int maxStructure)
            : this(name, LeatherMaterial.Static, maxStructure)
        {
        }

        public Boots(string name, Material itemMaterial, int maxStructure)
            : base(@"Boots", ItemSlot.FeetSlot)
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
            BaseWeight = 1d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"boots";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Total);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool UnslottingProvokes => true;
    }
}
