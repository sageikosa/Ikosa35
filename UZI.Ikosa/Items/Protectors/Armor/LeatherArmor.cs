using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Leather Armor", @"Light AR:+2 Check:0 MaxDex:6 SpellFail:10%", @"leather_armor")
    ]
    public class LeatherArmor : ArmorBase
    {
        public LeatherArmor()
            : base(@"Leather Armor", ArmorProficiencyType.Light, 2, 6, 0, 10)
        {
            Init(10m, 15d, Materials.LeatherMaterial.Static);
        }

        public LeatherArmor(Type bodyType)
            : base(@"Leather Armor", ArmorProficiencyType.Light, 2, 6, 0, 10, bodyType)
        {
            Init(10m, 15d, Materials.LeatherMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"leather_armor"; } }
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
