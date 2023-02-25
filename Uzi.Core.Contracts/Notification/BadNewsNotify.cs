using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class BadNewsNotify : SysNotify
    {
        public BadNewsNotify(Guid principalID, string topic, params Info[] info)
            : base(topic, info)
        {
            PrincipalID = principalID;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }
    }
}
