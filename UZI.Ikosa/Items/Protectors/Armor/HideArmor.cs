using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Hide Armor", @"Medium AR:+3 Check:-3 MaxDex:4 SpellFail:20% Speed-Reduced", @"hide_armor")
    ]
    public class HideArmor : ArmorBase
    {
        public HideArmor()
            : base(@"Hide Armor", ArmorProficiencyType.Medium, 3, 4, -3, 20)
        {
            Init(15m, 25d, Materials.HideMaterial.Static);
        }

        public HideArmor(Type bodyType)
            : base(@"Hide Armor", ArmorProficiencyType.Medium, 3, 4, -3, 20, bodyType)
        {
            Init(15m, 25d, Materials.HideMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"hide_armor"; } }
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
