using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public enum FlowState
    {
        [EnumMember]
        Normal,
        [EnumMember]
        Advancement,
        [EnumMember]
        Shutdown
    }
}
