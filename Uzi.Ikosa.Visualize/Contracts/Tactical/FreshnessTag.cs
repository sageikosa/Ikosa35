using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class FreshnessTag
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public FreshnessTime FreshTime { get; set; }
    }
}
