using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SizeInfo : ICloneable
    {
        public SizeInfo()
        {
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Order { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return new SizeInfo { Name = Name, Order = Order };
        }

        #endregion
    }
}
