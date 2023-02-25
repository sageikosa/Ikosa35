using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class VolatileValueInfo : Info, ICloneable
    {
        [DataMember]
        public int EffectiveValue { get; set; }

        public override object Clone()
            => new VolatileValueInfo
            {
                EffectiveValue = EffectiveValue
            };
    }
}
