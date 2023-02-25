using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LivingCreatureTargetTypeInfo : TargetTypeInfo
    {
        public LivingCreatureTargetTypeInfo()
            : base()
        {
        }
    }
}
