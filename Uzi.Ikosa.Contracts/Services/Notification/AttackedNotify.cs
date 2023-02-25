using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AttackedNotify : SysNotify
    {
        public AttackedNotify(Guid principalID, string topic, ObservedActivityInfo activity, params Info[] infos)
            : base(topic, infos)
        {
            PrincipalID = principalID;
            ObservedActivity = activity;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }

        [DataMember]
        public ObservedActivityInfo ObservedActivity { get; set; }
    }
}
