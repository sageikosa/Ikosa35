using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CapacityBudgetInfo : BudgetItemInfo
    {
        [DataMember]
        public int Capacity { get; set; }
        [DataMember]
        public int Available { get; set; }
    }
}
