using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CreatureTrackerInfo
    {
        [DataMember]
        public CreatureLoginInfo CreatureLoginInfo { get; set; }

        [DataMember]
        public UserInfo[] UserInfos { get; set; }

        [DataMember]
        public LocalActionBudgetInfo LocalActionBudgetInfo { get; set; }

        public Guid ID => CreatureLoginInfo?.ID ?? Guid.Empty;
    }
}
