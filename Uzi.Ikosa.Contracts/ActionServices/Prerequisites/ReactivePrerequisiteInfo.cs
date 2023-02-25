using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ReactivePrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public ActionInfo[] ReactiveActions { get; set; }
        [DataMember]
        public Info TriggeringCondition { get; set; }
        [DataMember]
        public ActivityInfo ResponseActivity { get; set; }
    }
}
