using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DocPhysicalStructure 
    {
        [DataMember]
        public DocSection DocSection { get; set; }
    }
}
