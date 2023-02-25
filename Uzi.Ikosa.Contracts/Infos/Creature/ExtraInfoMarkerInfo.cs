using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ExtraInfoMarkerInfo : ExtraInfoInfo
    {
        [DataMember]
        public bool DirectionOnly { get; set; }

        [DataMember]
        public CubicInfo Region { get; set; }

        [DataMember]
        public AuraStrength Strength { get; set; }
    }
}
