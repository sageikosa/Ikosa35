using System;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class ColdResistor: EnergyResistor
    {
        protected ColdResistor(int amount)
            : base(typeof(ColdResistor), EnergyType.Cold, amount)
        {
        }
    }

    [Serializable]
    [MagicAugmentRequirement(3, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ColdResistorLow : ColdResistor
    {
        public ColdResistorLow()
            : base(10)
        {
        }

        public override object Clone() { return new ColdResistorLow(); }
    }

    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ColdResistorMedium : ColdResistor
    {
        public ColdResistorMedium()
            : base(20)
        {
        }

        public override object Clone() { return new ColdResistorMedium(); }
    }

    [Serializable]
    [MagicAugmentRequirement(11, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class ColdResistorHigh : ColdResistor
    {
        public ColdResistorHigh()
            : base(30)
        {
        }

        public override object Clone() { return new ColdResistorHigh(); }
    }
}
