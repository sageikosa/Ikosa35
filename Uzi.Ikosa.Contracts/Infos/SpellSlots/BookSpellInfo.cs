using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BookSpellInfo : ClassSpellInfo
    {
        [DataMember]
        public Guid Owner { get; set; }
    }
}
