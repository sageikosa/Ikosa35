using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RequirementKeyInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int IndexKey { get; set; }
    }
}
