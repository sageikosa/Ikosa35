using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Intended to be used as a source for adjuncts, reach effects and interactions [InteractData, ICore, IMagicAura]
    /// </summary>
    [Serializable]
    public class SuperNaturalPowerActionSource : MagicPowerActionSource, ISuperNaturalPowerActionSource
    {
        public SuperNaturalPowerActionSource(IPowerClass powerClass, int powerLevel, ISuperNaturalPowerActionDef superNaturalDef) :
            base(powerClass, powerLevel, superNaturalDef)
        {
        }

        public ISuperNaturalPowerActionDef SuperNaturalPowerActionDef { get { return PowerActionDef as ISuperNaturalPowerActionDef; } }

        public override void UsePower()
        {
            SuperNaturalPowerActionDef.MagicBattery.UseCharges(SuperNaturalPowerActionDef.PowerCost);
        }

        public override string DisplayName { get { return SuperNaturalPowerActionDef.DisplayName; } }
    }
}
