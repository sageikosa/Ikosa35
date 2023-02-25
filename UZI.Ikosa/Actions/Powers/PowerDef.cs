using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class PowerDef<PowerSrc> : IPowerDef
        where PowerSrc : IPowerSource
    {
        protected PowerDef()
        {
        }

        public abstract string DisplayName { get; }
        public abstract string Description { get; }

        public virtual string Key 
            => GetType().FullName;

        public virtual IEnumerable<Descriptor> Descriptors 
            => Enumerable.Empty<Descriptor>();

        public virtual IPowerDef ForPowerSource() 
            => this;

        /// <summary>Default: true if overlapping, or source-material to target-ethereal with force descriptor</summary>
        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
            => source.HasOverlappingPresence(target)
            || (source.HasMaterialPresence() && target.HasEtherealPresence() && Descriptors.OfType<Force>().Any());

        public abstract PowerDefInfo ToPowerDefInfo();
    }
}
