using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Splint Mail", @"Heavy AR:+6 Check:-7 MaxDex:0 SpellFail:40% Speed-Reduced", @"splint_mail")
    ]
    public class SplintMail : ArmorBase
    {
        public SplintMail()
            : base(@"Splint mail", ArmorProficiencyType.Heavy, 6, 0, -7, 40)
        {
            Init(200.0M, 45d, Materials.SteelMaterial.Static);
        }

        public SplintMail(Type bodyType)
            : base(@"Splint mail", ArmorProficiencyType.Heavy, 6, 0, -7, 40, bodyType)
        {
            Init(200.0M, 45d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey => @"splint_mail";
        public override ActionTime SlottingTime => new ActionTime(4 * Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Minute.UnitFactor);
    }
}
