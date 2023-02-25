using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CoinTradeInfo : ICloneable
    {
        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public CoinTypeInfo CoinType { get; set; }

        public object Clone()
            => new CoinTradeInfo
            {
                Count = Count,
                CoinType = CoinType.Clone() as CoinTypeInfo
            };
    }
}
