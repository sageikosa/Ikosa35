using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Padded Armor", @"Light AR:+1 Check:0 MaxDex:8 SpellFail:5%", @"padded_armor")
    ]
    public class PaddedArmor : ArmorBase
    {
        public PaddedArmor()
            : base("Padded Armor", ArmorProficiencyType.Light, 1, 8, 0, 5)
        {
            Init(5m, 10d, Materials.ClothMaterial.Static);
        }

        public PaddedArmor(Type bodyType)
            : base("Padded Armor", ArmorProficiencyType.Light, 1, 8, 0, 5, bodyType)
        {
            Init(5m, 10d, Materials.ClothMaterial.Static);
        }

        protected override string ClassIconKey { get { return @"padded_armor"; } }
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
