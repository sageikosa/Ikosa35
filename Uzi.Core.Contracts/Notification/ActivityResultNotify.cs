using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ActivityResultNotify : SysNotify
    {
        public ActivityResultNotify(ObservedActivityInfo activity, params Info[] infos)
            : base(@"Activity Info", infos)
        {
            Activity = activity;
        }

        [DataMember]
        public ObservedActivityInfo Activity { get; set; }
    }
}
