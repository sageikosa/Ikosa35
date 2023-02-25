using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    [Serializable]
    public class ExtraInfoSource : Info
    {
        [DataMember]
        public Guid ID { get; set; }
        // TODO: class name?
    }
}
