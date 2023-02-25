using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Unarmored Clothing", @"None AR:+0 Check:0", @"clothing")
    ]
    public class Clothing : ArmorBase
    {
        public Clothing()
            : base(@"Clothing", ArmorProficiencyType.None, 0, 1000, 0, 0)
        {
            Init(10m, 8d, Materials.ClothMaterial.Static);
        }

        public Clothing(Type bodyType)
            : base(@"Clothing", ArmorProficiencyType.Light, 0, 1000, 0, 0, bodyType)
        {
            Init(10m, 15d, Materials.LeatherMaterial.Static);
        }

        protected override string ClassIconKey => @"clothing"; 
        public override ActionTime SlottingTime => new ActionTime(Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime => new ActionTime(Time.Minute.UnitFactor);
    }
}
