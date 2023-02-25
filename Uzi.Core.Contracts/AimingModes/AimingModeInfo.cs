using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AimingModeInfo
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Preposition { get; set; }
        [DataMember]
        public int ClassLevel { get; set; }
        [DataMember]
        public double MinimumAimingModes { get; set; }
        [DataMember]
        public double MaximumAimingModes { get; set; }
    }
}
