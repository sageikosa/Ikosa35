using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Used for General Roll, Caster Level, Save, and Success Check</summary>
    [DataContract(Namespace = Statics.Namespace)]
    public class RollPrerequisiteInfo : ValuePrerequisiteInfo
    {
        [DataMember]
        public string Expression { get; set; }
        [DataMember]
        public int? SingletonSides { get; set; }
    }
}