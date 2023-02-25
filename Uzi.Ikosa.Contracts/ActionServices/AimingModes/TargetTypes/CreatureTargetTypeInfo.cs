using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CreatureTargetTypeInfo : TargetTypeInfo
    {
        public CreatureTargetTypeInfo()
            : base()
        {
        }
    }
}
