using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class StandDownGroupInfo
    {
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public List<CreatureLoginInfo> Creatures { get; set; }
    }
}
