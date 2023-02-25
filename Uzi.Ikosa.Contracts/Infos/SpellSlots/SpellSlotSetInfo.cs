using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellSlotSetInfo : Info
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public int SetIndex { get; set; }

        [DataMember]
        public List<SpellSlotLevelInfo> SlotLevels { get; set; }

        [DataMember]
        public List<ClassSpellInfo> AvailableSpells { get; set; }

        public string SetName { get => Message; set => Message = value; }

        public override object Clone()
            => new SpellSlotSetInfo
            {
                ID = ID,
                SetIndex = SetIndex,
                SetName = SetName,
                AvailableSpells = AvailableSpells.Select(_spell => _spell.Clone()).OfType<ClassSpellInfo>().ToList(),
                SlotLevels = SlotLevels.Select(_lvl => _lvl.Clone()).OfType<SpellSlotLevelInfo>().ToList()
            };
    }
}
