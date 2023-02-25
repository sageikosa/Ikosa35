using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DocScript
    {
        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public string Script { get; set; }
    }
}
