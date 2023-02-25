using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DocSection
    {
        [DataMember]
        public DocScript Script { get; set; }

        [DataMember]
        public DocPassage Heading { get; set; }

        [DataMember]
        public List<DocPassage> Passages { get; set; }

        [DataMember]
        public List<DocSection> Sections { get; set; }
    }
}
