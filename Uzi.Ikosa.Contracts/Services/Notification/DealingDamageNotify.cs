using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DealingDamageNotify : SysNotify
    {
        public DealingDamageNotify(Guid principalID, string topic, Info source, IEnumerable<DamageInfo> damages)
            : base(topic)
        {
            PrincipalID = principalID;
            Source = source;
            Damages = damages.ToList();
        }

        [DataMember]
        public Guid PrincipalID { get; set; }

        [DataMember]
        public Info Source { get; set; }

        [DataMember]
        public List<DamageInfo> Damages { get; set; }
    }
}
