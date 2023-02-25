using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AdvanceableCreature
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public int NumberPowerDice { get; set; }
        [DataMember]
        public decimal PowerDiceCount { get; set; }
        [DataMember]
        public List<AdvancementLogInfo> AdvancementLogInfos { get; set; }
    }
}
