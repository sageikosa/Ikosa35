using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Bracers : SlottedItemBase
    {
        public Bracers(string name, Material itemMaterial, int maxHealthPoints)
            : base(@"Bracers", ItemSlot.ArmsSlot)
        {
            InitItem(itemMaterial, maxHealthPoints);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxHP)
        {
            Sizer.NaturalSize = Size.Tiny;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            // TODO: ... break DC of 25.
            BaseWeight = 1d;
        }

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"bracers";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Total);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool UnslottingProvokes => true;
    }
}
