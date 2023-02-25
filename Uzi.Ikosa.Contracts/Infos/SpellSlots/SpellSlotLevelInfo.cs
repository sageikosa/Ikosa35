using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellSlotLevelInfo : Info
    {
        [DataMember]
        public int SlotLevel { get; set; }

        [DataMember]
        public List<SpellSlotInfo> SpellSlots { get; set; }

        public override object Clone()
            => new SpellSlotLevelInfo
            {
                SlotLevel = SlotLevel,
                Message = Message,
                SpellSlots = SpellSlots.Select(_slot => _slot.Clone()).OfType<SpellSlotInfo>().ToList()
            };
    }
}
