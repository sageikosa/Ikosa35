using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CoinTypeInfo : ICloneable
    {
        [DataMember]
        public string PreciousMetal { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string PluralName { get; set; }
        [DataMember]
        public decimal UnitFactor { get; set; }
        [DataMember]
        public double UnitWeight { get; set; }

        public object Clone()
            => new CoinTypeInfo
            {
                PreciousMetal = PreciousMetal,
                Name = Name,
                PluralName = PluralName,
                UnitFactor = UnitFactor,
                UnitWeight = UnitWeight
            };
    }
}
