using System.Collections.Generic;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerDef
    {
        string DisplayName { get; }
        string Description { get; }
        string Key { get; }

        bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target);

        /// <summary>Provides instance suitable for being used in a power source (possibly mutable if definition varies over use)</summary>
        IPowerDef ForPowerSource();

        /// <summary>Lists the default descriptors for a power definition.</summary>
        IEnumerable<Descriptor> Descriptors { get; }
        PowerDefInfo ToPowerDefInfo();
    }
}
