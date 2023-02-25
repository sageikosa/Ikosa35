using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public abstract class PrerequisiteInfo
    {
        // TODO: FallPre

        [DataMember]
        public Guid FulfillerID { get; set; }
        [DataMember]
        public Guid StepID { get; set; }
        [DataMember]
        public string BindKey { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsReady { get; set; }
        [DataMember]
        public bool IsSerial { get; set; }
    }
}
