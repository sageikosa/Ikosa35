using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DocTextContent : DocContent
    {
        [DataMember]
        public DocScript Script { get; set; }

        [DataMember]
        public string Text { get; set; }
    }
}
