using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PrepareSpellSlotsTargetInfo : AimTargetInfo
    {
        public PrepareSpellSlotsTargetInfo()
            : base()
        {
        }

        [DataMember]
        public List<SpellSlotSetInfo> SlotSets { get; set; }
    }
}
