using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Chain Shirt", @"Light AR:+4 Check:-2 MaxDex:4 SpellFail:20%", @"chain_shirt")
    ]
    public class ChainShirt : ArmorBase
    {
        public ChainShirt()
            : base(@"Chain Shirt", ArmorProficiencyType.Light, 4, 4, -2, 20)
        {
            Init(100m, 25d, Materials.SteelMaterial.Static);
        }

        public ChainShirt(Type bodyType)
            : base(@"Chain Shirt", ArmorProficiencyType.Light, 4, 4, -2, 20, bodyType)
        {
            Init(100m, 25d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"chain_shirt"; } }
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
