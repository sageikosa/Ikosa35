using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class IdentityInfo
    {
        [DataMember]
        public Guid InfoID { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public ObjectInfo ObjectInfo { get; set; }
    }
}
