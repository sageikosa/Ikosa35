using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PromptTurnTrackerPrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public List<Guid> Triggered { get; set; }

        [DataMember]
        public bool Done { get; set; }
    }
}
