using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ClassInfo
    {
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public string ClassName { get; set; }
        [DataMember]
        public byte PowerDieSize { get; set; }
        [DataMember]
        public double BaseAttackProgression { get; set; }
        [DataMember]
        public int SkillPointsPerLevel { get; set; }
        [DataMember]
        public bool GoodFortitude { get; set; }
        [DataMember]
        public bool GoodReflex { get; set; }
        [DataMember]
        public bool GoodWill { get; set; }
        [DataMember]
        public int CurrentLevel { get; set; }
        [DataMember]
        public VolatileValueInfo EffectiveLevel { get; set; }
    }
}
