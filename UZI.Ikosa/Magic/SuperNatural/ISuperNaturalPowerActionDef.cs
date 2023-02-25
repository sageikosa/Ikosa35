using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Magic
{
    public interface ISuperNaturalPowerActionDef : IMagicPowerActionDef, ICapabilityRoot
    {
        /// <summary>Magic Battery that control the use of the power</summary>
        IPowerBattery MagicBattery { get; }

        /// <summary>How much a standard use of the power costs</summary>
        int PowerCost { get; }

        /// <summary>Power Level for ability</summary>
        int PowerLevel { get; }

        /// <summary>Unmodified power</summary>
        SuperNaturalPowerActionDef SeedPowerDef { get; }
    }
}
