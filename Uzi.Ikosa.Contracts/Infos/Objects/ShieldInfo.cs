using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ShieldInfo : ProtectorInfo
    {
        #region construction
        public ShieldInfo()
            : base()
        {
        }

        protected ShieldInfo(ShieldInfo copySource)
            : base(copySource)
        {
            IsTower = copySource.IsTower;
            UseHandToCarry = copySource.UseHandToCarry;
        }
        #endregion

        [DataMember]
        public bool IsTower { get; set; }
        [DataMember]
        public bool UseHandToCarry { get; set; }

        public override object Clone()
        {
            return new ShieldInfo(this);
        }
    }
}
