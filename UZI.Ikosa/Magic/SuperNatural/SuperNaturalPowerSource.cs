using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SuperNaturalPowerSource : MagicPowerSource, ISuperNaturalPowerSource
    {
        public SuperNaturalPowerSource(IPowerClass powerClass, int powerLevel, ISuperNaturalPowerDef superNaturalDef) :
            base(powerClass, powerLevel, superNaturalDef)
        {
        }

        public ISuperNaturalPowerDef SuperNaturalPowerDef { get { return PowerDef as ISuperNaturalPowerDef; } }
    }
}
