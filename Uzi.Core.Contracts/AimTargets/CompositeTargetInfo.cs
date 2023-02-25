using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CompositeTargetInfo : AimTargetInfo
    {
        public CompositeTargetInfo()
            : base()
        {
        }

        [DataMember]
        public List<AimTargetInfo> Components { get; set; }
    }
}
