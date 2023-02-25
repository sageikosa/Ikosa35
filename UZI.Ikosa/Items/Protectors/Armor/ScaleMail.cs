using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Scale Mail", @"Medium AR:+4 Check:-4 MaxDex:3 SpellFail:25% Speed-Reduced", @"scale_mail")
    ]
    public class ScaleMail : ArmorBase
    {
        public ScaleMail()
            : base(@"Scale mail", ArmorProficiencyType.Medium, 4, 3, -4, 25)
        {
            Init(50m, 30d, Materials.SteelMaterial.Static);
        }

        public ScaleMail(Type bodyType)
            : base(@"Scale mail", ArmorProficiencyType.Medium, 4, 3, -4, 25, bodyType)
        {
            Init(50m, 30d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey => @"scale_mail"; 
        public override ActionTime SlottingTime => new ActionTime(4 * Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Minute.UnitFactor);
    }
}
