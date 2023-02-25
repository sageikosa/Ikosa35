using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class RemoteMoveInfo : Info
    {
        [DataMember]
        public AdjunctInfo ControlAdjunct { get; set; }

        [DataMember]
        public MovementInfo RemoteMove { get; set; }

        [DataMember]
        public Info RemoteMoveTarget { get; set; }
    }
}
