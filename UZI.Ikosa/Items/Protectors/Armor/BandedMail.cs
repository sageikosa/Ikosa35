using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Banded Mail", @"Heavy AR:+6 Check:-6 MaxDex:1 SpellFail:35% Speed-Reduced", @"banded_mail")
    ]
    public class BandedMail : ArmorBase
    {
        public BandedMail()
            : base(@"Banded mail", ArmorProficiencyType.Heavy, 6, 1, -6, 35)
        {
            Init(250.0M, 35d, Materials.SteelMaterial.Static);
        }

        public BandedMail(Type bodyType)
            : base(@"Banded mail", ArmorProficiencyType.Heavy, 6, 1, -6, 35, bodyType)
        {
            Init(250.0M, 35d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"banded_mail"; } }
        public override ActionTime SlottingTime => new ActionTime(4 * Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Minute.UnitFactor);
    }
}
