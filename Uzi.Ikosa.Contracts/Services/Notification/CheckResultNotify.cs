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
    public class CheckResultNotify : SysNotify
    {
        public CheckResultNotify(Guid principalID, string topic, bool success, params Info[] infos)
            : base(topic, infos)
        {
            PrincipalID = principalID;
            Success = success;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }

        [DataMember]
        public bool Success { get; set; }
    }
}
