using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts.Host
{
    [DataContract(Namespace = Statics.Namespace)]
    public class UserInfo
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public CreatureLoginInfo[] CreatureInfos { get; set; }
        [DataMember]
        public bool IsMaster { get; set; }
        [DataMember]
        public bool IsDisabled { get; set; }

        // NON-Contracted
        public ILoginCallback Notifier { get; set; }
    }
}
