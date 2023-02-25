using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Headband : SlottedItemBase
    {
        public Headband(string name, Material itemMaterial, int maxHealthPoints)
            : base(@"Headband", ItemSlot.HeadSlot)
        {
            InitItem(itemMaterial, maxHealthPoints);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxHP)
        {
            Sizer.NaturalSize = Size.Miniature;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            // TODO: ... break DC of 25.
            BaseWeight = 0.125d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"headband";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
