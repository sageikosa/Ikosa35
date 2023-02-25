using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CheckNotify : SysNotify
    {
        public CheckNotify(
            Guid checkPrincipal, string checkTitle,
            Guid opposedPrincipal, string opposedTitle)
            : base(checkTitle)
        {
            CheckInfo = new DeltaCalcInfo(checkPrincipal, checkTitle);
            OpposedInfo = new DeltaCalcInfo(opposedPrincipal, opposedTitle);
        }

        [DataMember]
        public DeltaCalcInfo CheckInfo { get; set; }

        [DataMember]
        public DeltaCalcInfo OpposedInfo { get; set; }
    }
}
