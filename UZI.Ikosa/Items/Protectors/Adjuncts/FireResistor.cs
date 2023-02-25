using System;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class FireResistor: EnergyResistor
    {
        protected FireResistor(int amount)
            : base(typeof(FireResistor), EnergyType.Fire, amount)
        {
        }
    }

    [Serializable]
    [MagicAugmentRequirement(3, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class FireResistorLow : FireResistor
    {
        public FireResistorLow()
            : base(10)
        {
        }

        public override object Clone() { return new FireResistorLow(); }
    }

    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class FireResistorMedium : FireResistor
    {
        public FireResistorMedium()
            : base(20)
        {
        }

        public override object Clone() { return new FireResistorMedium(); }
    }

    [Serializable]
    [MagicAugmentRequirement(11, typeof(CraftMagicArmsAndArmorFeat), typeof(ResistEnergy))]
    public class FireResistorHigh : FireResistor
    {
        public FireResistorHigh()
            : base(30)
        {
        }

        public override object Clone() { return new FireResistorHigh(); }
    }
}
