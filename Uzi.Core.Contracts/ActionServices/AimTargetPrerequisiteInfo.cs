using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AimTargetPrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public AimingModeInfo AimingMode { get; set; }
        [DataMember]
        public AimTargetInfo[] AimTargets { get; set; }
    }
}
