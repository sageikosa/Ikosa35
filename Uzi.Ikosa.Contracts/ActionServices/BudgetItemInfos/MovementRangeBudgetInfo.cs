using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MovementRangeBudgetInfo : BudgetItemInfo
    {
        [DataMember]
        public double Double { get; set; }

        [DataMember]
        public double Remaining { get; set; }
    }
}
