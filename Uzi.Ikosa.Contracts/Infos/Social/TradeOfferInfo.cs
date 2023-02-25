using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class TradeOfferInfo
    {
        [DataMember]
        public List<CoreInfo> Items { get; set; }
        [DataMember]
        public List<CoinTradeInfo> Coins { get; set; }
    }
}
