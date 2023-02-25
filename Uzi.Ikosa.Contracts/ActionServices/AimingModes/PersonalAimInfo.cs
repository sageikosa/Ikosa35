using System.Collections.Generic;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PersonalAimInfo : AimingModeInfo
    {
        [DataMember]
        public List<AwarenessInfo> ValidTargets { get; set; }
    }
}
