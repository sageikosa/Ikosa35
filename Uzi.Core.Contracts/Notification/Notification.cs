using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class Notification
    {
        [DataMember]
        public Guid NotifyID { get; set; }
        [DataMember]
        public List<SysNotify> Notifications { get; set; }
    }
}
