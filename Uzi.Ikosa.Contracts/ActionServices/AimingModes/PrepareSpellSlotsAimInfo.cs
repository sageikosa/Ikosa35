using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PrepareSpellSlotsAimInfo : AimingModeInfo
    {
        [DataMember]
        public bool AbandonRecharge { get; set; }

        [DataMember]
        public CasterClassInfo CasterClass { get; set; }

        [DataMember]
        public List<SpellSlotSetInfo> SlotSets { get; set; }

        [DataMember]
        public List<MetaMagicInfo> AvailableMetaMagics { get; set; }
    }
}
