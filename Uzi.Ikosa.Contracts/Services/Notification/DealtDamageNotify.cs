using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DealtDamageNotify : SysNotify
    {
        public DealtDamageNotify(Guid principalID, string topic, IEnumerable<DamageInfo> damages, params Info[] infos)
            : base(topic, infos)
        {
            PrincipalID = principalID;
            Damages = damages.ToList();
        }

        [DataMember]
        public Guid PrincipalID { get; set; }

        [DataMember]
        public List<DamageInfo> Damages { get; set; }
    }
}
