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
    public class RollNotify : SysNotify
    {
        public RollNotify(Guid principalID, Info source, RollerLog rollerLog)
            : base(@"Roll", source)
        {
            PrincipalID = principalID;
            RollerLog = rollerLog;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }
        [DataMember]
        public RollerLog RollerLog { get; set; }
    }
}
