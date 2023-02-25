using System;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class SonicResistor: EnergyResistor
    {
        protected SonicResistor(int amount)
            : base(typeof(SonicResistor), EnergyType.Sonic, amount)
        {
        }
    }

    [Serializable]
    [MagicAugmentRequirement(3, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class SonicResistorLow : SonicResistor
    {
        public SonicResistorLow()
            : base(10)
        {
        }

        public override object Clone() { return new SonicResistorLow(); }
    }

    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class SonicResistorMedium : SonicResistor
    {
        public SonicResistorMedium()
            : base(20)
        {
        }

        public override object Clone() { return new SonicResistorMedium(); }
    }

    [Serializable]
    [MagicAugmentRequirement(11, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class SonicResistorHigh : SonicResistor
    {
        public SonicResistorHigh()
            : base(30)
        {
        }

        public override object Clone() { return new SonicResistorHigh(); }
    }
}
