using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class HealthPointInfo
    {
        [DataMember]
        public int Maximum { get; set; }
        [DataMember]
        public int Current { get; set; }
        [DataMember]
        public int NonLethal { get; set; }
        [DataMember]
        public int Temporary { get; set; }
        [DataMember]
        public int FromPowerDice { get; set; }
        [DataMember]
        public int FromConstitution { get; set; }
        [DataMember]
        public int Extra { get; set; }
        [DataMember]
        public int DeadValue { get; set; }
        [DataMember]
        public int MassiveDamage { get; set; }
    }
}
