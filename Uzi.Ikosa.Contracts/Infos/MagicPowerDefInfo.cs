using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MagicPowerDefInfo : PowerDefInfo
    {
        public MagicPowerDefInfo()
            : base()
        {
        }

        protected MagicPowerDefInfo(MagicPowerDefInfo copySource)
            : base(copySource)
        {
            MagicStyle = copySource.MagicStyle;
        }

        [DataMember]
        public string MagicStyle { get; set; }

        public override object Clone()
            => new MagicPowerDefInfo(this);
    }
}
