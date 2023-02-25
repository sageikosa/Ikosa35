using System;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class AcidResistor : EnergyResistor
    {
        public AcidResistor(int amount)
            : base(typeof(AcidResistor), EnergyType.Acid, amount)
        {
        }
    }

    [Serializable]
    [MagicAugmentRequirement(3, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class AcidResistorLow : AcidResistor
    {
        public AcidResistorLow()
            : base(10)
        {
        }

        public override object Clone() { return new AcidResistorLow(); }
    }

    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class AcidResistorMedium : AcidResistor
    {
        public AcidResistorMedium()
            : base(20)
        {
        }

        public override object Clone() { return new AcidResistorMedium(); }
    }

    [Serializable]
    [MagicAugmentRequirement(11, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class AcidResistorHigh : AcidResistor
    {
        public AcidResistorHigh()
            : base(30)
        {
        }

        public override object Clone() { return new AcidResistorHigh(); }
    }
}
