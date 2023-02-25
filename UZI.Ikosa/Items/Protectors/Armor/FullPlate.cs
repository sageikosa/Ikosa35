using System;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Full Plate", @"Heavy AR:+8 Check:-6 MaxDex:1 SpellFail:35% Speed-Reduced", @"full_plate")
    ]
    public class FullPlate : ArmorBase
    {
        public FullPlate()
            : base(@"Full plate", ArmorProficiencyType.Heavy, 8, 1, -6, 35)
        {
            Init(1500M, 50d, Materials.SteelMaterial.Static);
        }

        public FullPlate(Type bodyType)
            : base(@"Full plate", ArmorProficiencyType.Heavy, 8, 1, -6, 35, bodyType)
        {
            Init(1500M, 50d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey => @"full_plate"; 
        public override ActionTime SlottingTime
            => new ActionTime(4 * Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime
            => new ActionTime((DieRoller.RollDie(CreaturePossessor?.ID ?? Guid.Empty, 4, @"Unslot", @"Minutes") + 1) * Time.Minute.UnitFactor);
    }
}
