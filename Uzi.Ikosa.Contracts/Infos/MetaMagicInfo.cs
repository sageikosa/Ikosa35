using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaMagicInfo : Info
    {
        [DataMember]
        public string MetaTag { get; set; }

        [DataMember]
        public int SlotAdjustment { get; set; }

        [DataMember]
        public Guid PresenterID { get; set; }

        public override object Clone()
            => new MetaMagicInfo
            {
                Message = Message,
                MetaTag = MetaTag,
                SlotAdjustment = SlotAdjustment,
                PresenterID = PresenterID
            };
    }
}
