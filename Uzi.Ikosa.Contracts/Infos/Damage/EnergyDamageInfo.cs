using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class EnergyDamageInfo : DamageInfo
    {
        [DataMember]
        public EnergyType Energy { get; set; }
    }
}
