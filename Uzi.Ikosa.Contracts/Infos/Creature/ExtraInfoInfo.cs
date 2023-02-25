using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ExtraInfoInfo
    {
        public ExtraInfoInfo()
        {
        }

        [DataMember]
        public Guid? ProviderID { get; set; }

        [DataMember]
        public Guid SourceID { get; set; }

        [DataMember]
        public string SourceTitle { get; set; }

        [DataMember]
        public Info[] Informations { get; set; }
    }
}
