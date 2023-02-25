using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class DeltableInfo : VolatileValueInfo
    {
        public DeltableInfo()
        {
        }
        public DeltableInfo(int baseValue)
        {
            BaseDoubleValue = baseValue;
            BaseValue = baseValue;
            EffectiveValue = baseValue;
            DeltaDescriptions = new Collection<string>();
        }
        [DataMember]
        public double BaseDoubleValue { get; set; }
        [DataMember]
        public int BaseValue { get; set; }
        [DataMember]
        public Collection<string> DeltaDescriptions { get; set; }

        #region ICloneable Members

        public override object Clone()
        {
            return new DeltableInfo
            {
                BaseDoubleValue = BaseDoubleValue,
                BaseValue = BaseValue,
                EffectiveValue = EffectiveValue,
                DeltaDescriptions = new Collection<string>(DeltaDescriptions.ToList())
            };
        }

        #endregion
    }
}
