using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable,
    ItemInfo(@"Devotional Symbol", @"Symbol of a divine focus", @"divine_focus")]
    public class DevotionalSymbol : SlottedItemBase
    {
        public DevotionalSymbol(string name, string devotion, Material itemMaterial, int maxHealthPoints)
            : base(@"Devotional Symbol", ItemSlot.DevotionalSymbol)
        {
            InitItem(itemMaterial, maxHealthPoints);
            _Devotion = devotion;
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxHP)
        {
            Sizer.NaturalSize = Size.Fine;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            // TODO: ... break DC of 25.
            Price.CorePrice = itemMaterial.Name == Silver.Static.Name ? 25m : 1m;
            BaseWeight = itemMaterial.Name == Silver.Static.Name ? 1d : 0.25d;
        }

        private string _Devotion;

        public string Devotion => _Devotion;

        public override bool IsTransferrable
            => true;

        protected override string ClassIconKey
            => @"divine_focus";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
