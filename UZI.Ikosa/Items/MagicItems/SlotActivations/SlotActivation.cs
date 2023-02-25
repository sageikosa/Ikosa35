using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// <para>Used for magic item powers that activate when the item is slotted</para>
    /// <para>Activated when the item is placed in a slot, and deactivated when unslotted.</para>
    /// <para>[Adjunct, IMagicAura, IIdentification, IProtectable, ICore]</para>
    /// </summary>
    [Serializable]
    public abstract class SlotActivation : SlotConnectedAugmentation, IIdentification, IProtectable, ICore
    {
        protected SlotActivation(object source, bool affinity)
            : base(source, affinity)
        {
        }

        public abstract IEnumerable<Info> IdentificationInfos { get; }

        public bool IsExposedTo(Creature critter)
            => this.HasExposureTo(critter);
    }
}
