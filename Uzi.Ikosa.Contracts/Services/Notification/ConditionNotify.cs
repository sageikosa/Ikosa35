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
    public class ConditionNotify : SysNotify
    {
        public ConditionNotify(Guid principalID, string condition, bool isEnding)
            : base(@"Condition")
        {
            PrincipalID = principalID;
            Condition = condition;
            IsEnding = isEnding;
        }

        [DataMember]
        public Guid PrincipalID { get; set; }

        [DataMember]
        public string Condition { get; set; }

        [DataMember]
        public bool IsEnding { get; set; }
    }
}
