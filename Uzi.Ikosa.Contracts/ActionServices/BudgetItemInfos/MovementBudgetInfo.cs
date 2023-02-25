using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MovementBudgetInfo : BudgetItemInfo
    {
        [DataMember]
        public int? Heading { get; set; }
        [DataMember]
        public bool HasMoved { get; set; }
        [DataMember]
        public bool CanStillMove { get; set; }
    }
}
