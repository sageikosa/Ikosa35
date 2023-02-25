using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    [Flags]
    public enum PlanarPresence
    {
        None = 0,
        Material = 1,
        Ethereal = 2,
        /// <summary>Used for observation and detect lines when on ethereal plane</summary>
        Both = Material | Ethereal
    }

    public static class PlanarPresenceHelper
    {
        public static bool HasMaterialPresence(this PlanarPresence self)
            => (self & PlanarPresence.Material) == PlanarPresence.Material;

        public static bool HasEtherealPresence(this PlanarPresence self)
            => (self & PlanarPresence.Ethereal) == PlanarPresence.Ethereal;

        public static bool HasOverlappingPresence(this PlanarPresence self, PlanarPresence other)
            => (self & other) != PlanarPresence.None;
    }
}
