using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Studded Leather Armor", @"Light AR:+3 Check:-1 MaxDex:5 SpellFail:15%", @"studded_leather_armor")
    ]
    public class StuddedLeatherArmor : ArmorBase
    {
        public StuddedLeatherArmor()
            : base(@"Studded Leather Armor", ArmorProficiencyType.Light, 3, 5, -1, 15)
        {
            Init(25m, 20d, Materials.LeatherMaterial.Static);
        }

        public StuddedLeatherArmor(Type bodyType)
            : base("Studded Leather Armor", ArmorProficiencyType.Light, 3, 5, -1, 15, bodyType)
        {
            Init(25m, 20d, Materials.LeatherMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"studded_leather_armor"; } }
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
