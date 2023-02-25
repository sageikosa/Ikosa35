using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts.Host
{
    [DataContract(Namespace = Statics.Namespace)]
    public class UserMessage
    {
        [DataMember]
        public string FromUser { get; set; }
        [DataMember]
        public string ToUser { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public DateTime Created { get; set; }
        [DataMember]
        public DateTime? Delivered { get; set; }
        [DataMember]
        public bool IsPublic { get; set; }
    }
}
