using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DeltaInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Value { get; set; }
    }
}
