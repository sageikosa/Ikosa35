using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AttackAimInfo : RangedAimInfo
    {
        [DataMember]
        public int CriticalThreatStart { get; set; }
        [DataMember]
        public TargetTypeInfo[] ValidTargetTypes { get; set; }
        [DataMember]
        public AttackImpact Impact { get; set; }

        /// <summary>Indicates to client to aim with a target cell so that indirect secondary effects have a capture zone</summary>
        [DataMember]
        public bool UseCellForIndirect { get; set; }

        [DataMember]
        public Lethality LethalOption { get; set; }
        [DataMember]
        public bool UseHiddenRolls { get; set; }

        /// <summary>This aim should only target cells...</summary>
        public bool IsCellAimOnly { get { return (ValidTargetTypes == null) || !ValidTargetTypes.Any(); } }
    }
}
