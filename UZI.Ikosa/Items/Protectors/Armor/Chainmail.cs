using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Chainmail", @"Medium AR:+5 Check:-5 MaxDex:2 SpellFail:30% Speed-Reduced", @"chain_mail")
    ]
    public class Chainmail : ArmorBase
    {
        public Chainmail()
            : base(@"Chainmail", ArmorProficiencyType.Medium, 5, 2, -5, 30)
        {
            Init(150m, 40d, Materials.SteelMaterial.Static);
        }

        public Chainmail(Type bodyType)
            : base(@"Chainmail", ArmorProficiencyType.Medium, 5, 2, -5, 30, bodyType)
        {
            Init(150m, 40d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"chain_mail"; } }
        public override ActionTime SlottingTime => new ActionTime(4 * Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Minute.UnitFactor);
    }
}
