using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Energy resistance (use as source for all deltas!)
    /// </summary>
    [Serializable]
    public class EnergyResistance : ConstDeltable
    {
        public EnergyResistance(EnergyType energyType)
            : base(0)
        {
            EnergyType = energyType;
        }

        public EnergyType EnergyType { get; private set; }
        public string Description => $@"Resist {EnergyType} {EffectiveValue}";
    }
}
