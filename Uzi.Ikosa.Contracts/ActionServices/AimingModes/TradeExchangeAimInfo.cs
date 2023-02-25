using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class TradeExchangeAimInfo : AimingModeInfo
    {
        [DataMember]
        public TradeOfferInfo Offer { get; set; }
        [DataMember]
        public bool IsAccepted { get; set; }
        [DataMember]
        public CoreInfo Partner { get; set; }
        [DataMember]
        public TradeOfferInfo PartnerOffer { get; set; }
        [DataMember]
        public bool PartnerAccepted { get; set; }
    }
}
