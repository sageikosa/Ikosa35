using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class OpportunisticPrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public ActionInfo[] AttackActions { get; set; }
        [DataMember]
        public ObservedActivityInfo ActivityInfo { get; set; }
        [DataMember]
        public ActivityInfo OpportunisticActivity { get; set; }
    }
}
