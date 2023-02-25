using System;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [
    Serializable,
    ItemInfo(@"Half Plate", @"Heavy AR:+7 Check:-7 MaxDex:0 SpellFail:40% Speed-Reduced", @"half_plate")
    ]
    public class HalfPlate : ArmorBase
    {
        public HalfPlate()
            : base(@"Half-plate", ArmorProficiencyType.Heavy, 7, 0, -7, 40)
        {
            Init(600M, 50d, Materials.SteelMaterial.Static);
        }

        public HalfPlate(Type bodyType)
            : base(@"Half-plate", ArmorProficiencyType.Heavy, 7, 0, -7, 40, bodyType)
        {
            Init(600M, 50d, Materials.SteelMaterial.Static);
        }

        protected override string ClassIconKey => @"half_plate";
        public override ActionTime SlottingTime
           => new ActionTime(4 * Time.Minute.UnitFactor);
        public override ActionTime UnslottingTime
            => new ActionTime((DieRoller.RollDie(CreaturePossessor?.ID ?? Guid.Empty, 4, @"Unslot", @"Rounds") + 1) * Time.Minute.UnitFactor);
    }
}
