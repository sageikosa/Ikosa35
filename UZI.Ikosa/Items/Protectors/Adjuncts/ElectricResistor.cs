using System;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class ElectricResistor: EnergyResistor
    {
        protected ElectricResistor(int amount)
            : base(typeof(ElectricResistor), EnergyType.Electric, amount)
        {
        }
    }

    [Serializable]
    [MagicAugmentRequirement(3, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ElectricResistorLow : ElectricResistor
    {
        public ElectricResistorLow()
            : base(10)
        {
        }

        public override object Clone() { return new ElectricResistorLow(); }
    }

    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ElectricResistorMedium : ElectricResistor
    {
        public ElectricResistorMedium()
            : base(20)
        {
        }

        public override object Clone() { return new ElectricResistorMedium(); }
    }

    [Serializable]
    [MagicAugmentRequirement(11, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ElectricResistorHigh : ElectricResistor
    {
        public ElectricResistorHigh()
            : base(30)
        {
        }

        public override object Clone() { return new ElectricResistorHigh(); }
    }
}
