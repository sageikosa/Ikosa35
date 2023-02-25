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
    public class CoreSelectPrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public Guid[] IDs { get; set; }
        [DataMember]
        public Guid? Selected { get; set; }
    }
}
