using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DeltaCalcNotify : SysNotify
    {
        public DeltaCalcNotify(Guid principalID, string title)
            : base(title)
        {
            DeltaCalc = new DeltaCalcInfo(principalID, title);
        }

        [DataMember]
        public DeltaCalcInfo DeltaCalc { get; set; }
    }
}
