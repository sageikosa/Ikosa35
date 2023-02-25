using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RollAimInfo : AimingModeInfo
    {
        [DataMember]
        public string RollerString { get; set; }
    }
}
