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
    public class GoodNewsNotify : SysNotify
    {
        public GoodNewsNotify(Guid principalID, string topic, Info info)
            : base(topic, info)
        {
            PrincipalID = principalID;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }
    }
}
