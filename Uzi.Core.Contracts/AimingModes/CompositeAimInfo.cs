using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CompositeAimInfo : AimingModeInfo
    {
        [DataMember]
        public List<AimingModeInfo> Components { get; set; }
    }
}
