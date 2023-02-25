using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ActionProviderInfo : Info
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public Guid PresenterID { get; set; }
        [DataMember]
        public string OrderKey { get; set; }
        [DataMember]
        public Info ProviderInfo { get; set; }
    }
}
