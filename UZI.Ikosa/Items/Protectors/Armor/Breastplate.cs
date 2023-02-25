using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Breastplate", @"Medium AR:+5 Check:-4 MaxDex:3 SpellFail:25% Speed:20", @"breastplate")
    ]
    public class Breastplate : ArmorBase
    {
        public Breastplate()
            : base(@"Breastplate", ArmorProficiencyType.Medium, 5, 3, -4, 25)
        {
            Init(200m, 30d, Materials.SteelMaterial.Static);
        }

        public Breastplate(Type bodyType)
            : base(@"Breastplate", ArmorProficiencyType.Medium, 5, 3, -4, 25, bodyType)
        {
            Init(200m, 30d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"breastplate"; } }
        public override ActionTime SlottingTime => new ActionTime(4 * Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Minute.UnitFactor);
    }
}
